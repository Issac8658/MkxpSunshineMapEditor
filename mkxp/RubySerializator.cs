using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RPG
{
	public static class RubySerializator
	{
		/*
		private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
		private static readonly Type[] _rubyClasses = [.. _assembly.GetTypes()
			.Where(type => type.IsClass && type.IsDefined(typeof(RubySerializableClassAttribute)))];
		*/
		private static readonly MethodInfo _variantDeserializeMethod = typeof(Variant).GetMethod(nameof(Variant.As));
		private static readonly MethodInfo _deserializeMethod = typeof(RubySerializator).GetMethod(nameof(DeserializeData));

		public static T DeserializeData<T>(Variant data) where T : RubySerializable, new()
		{
			Type targetType = typeof(T);

			Dictionary dictionaryData = data.AsGodotDictionary();
			if (dictionaryData.Count == 0)
			{
				GD.PushWarning($"Failed to deseralize, class \"{targetType.Name}\": Data is empty");
				return new T();
			}
			RubySerializableClassAttribute attribute = targetType.GetCustomAttribute<RubySerializableClassAttribute>();
			string rubyClassName = dictionaryData.GetValueOrDefault("__class__", "null").AsString();
			if (rubyClassName != attribute?.ClassName)
				GD.PushWarning($"Classes \"{rubyClassName}\" (Data) and \"{attribute?.ClassName}\" (Target) is different!");

			T result = new() { RawData = data };

			foreach (FieldInfo field in targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.IsDefined(typeof(RubySerializableAttribute)))
				{
					string rubyVariable = field.GetCustomAttribute<RubySerializableAttribute>().VariableName;
					if (dictionaryData.TryGetValue(rubyVariable, out Variant serializedFieldValue))
					{
						// if field is something like Array<RubySerializable>
						if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Array<>))
						{
							Type arrayElementsType = field.FieldType.GetGenericArguments()[0];
							bool isNestedArray = arrayElementsType.IsGenericType && arrayElementsType.GetGenericTypeDefinition() == typeof(Array<>);

							if (!(isNestedArray || TypeIsPrimitiveOrGodotNative(arrayElementsType)))
							{
								object typedGodotArray = Activator.CreateInstance(field.FieldType);
								MethodInfo addMethod = field.FieldType.GetMethod("Add");

								foreach (Variant value in serializedFieldValue.AsGodotArray())
									addMethod?.Invoke(typedGodotArray, [_deserializeMethod?.MakeGenericMethod(arrayElementsType)?.Invoke(null, [value])]);

								field.SetValue(result, typedGodotArray);
								continue;
							}
						}
						if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Godot.Collections.Dictionary<,>))
						{
							Type dictKeyType = field.FieldType.GetGenericArguments()[0];
							Type dictValueType = field.FieldType.GetGenericArguments()[1];

							if (!(TypeIsPrimitiveOrGodotNative(dictKeyType) && TypeIsPrimitiveOrGodotNative(dictValueType)))
							{
								Dictionary serializedFieldDict = serializedFieldValue.AsGodotDictionary();

								object typedGodotDict = Activator.CreateInstance(field.FieldType);
								MethodInfo addMethod = field.FieldType.GetMethod("Add", [dictKeyType, dictValueType]);

								foreach(Variant key in serializedFieldDict.Keys)
								{
									object resKey;
									object resVal;

									if (dictKeyType.IsSubclassOf(typeof(RubySerializable)))
										resKey = _deserializeMethod.MakeGenericMethod(dictKeyType)?.Invoke(null, [key]);
									else
										resKey = _variantDeserializeMethod.MakeGenericMethod(dictKeyType)?.Invoke(key, null);

									if (dictValueType.IsSubclassOf(typeof(RubySerializable)))
										resVal = _deserializeMethod.MakeGenericMethod(dictValueType)?.Invoke(null, [serializedFieldDict[key]]);
									else
										resVal = _variantDeserializeMethod.MakeGenericMethod(dictValueType)?.Invoke(serializedFieldDict[key], null);
									
									addMethod?.Invoke(typedGodotDict, [resKey, resVal]);
								}
								field.SetValue(result, typedGodotDict);
								continue;
							}
						}
						// else if field is RubySerializable
						if (field.FieldType.IsSubclassOf(typeof(RubySerializable)))
						{
							field.SetValue(result, _deserializeMethod.MakeGenericMethod(field.FieldType)?.Invoke(null, [serializedFieldValue]));
							continue;
						}
						// else convert Variant to field's type

						MethodInfo deserializeMethod = _variantDeserializeMethod.MakeGenericMethod(field.FieldType);
						object deserializedValue = deserializeMethod.Invoke(serializedFieldValue, null);

						field.SetValue(result, deserializedValue);
					}
				}
			}

			return result;
		}

		private static bool TypeIsPrimitiveOrGodotNative(Type type) =>
			type.IsPrimitive || type == typeof(string) || type.Namespace.StartsWith("Godot");
		/*
		public static Type GetClassFromRubyName(string rubyName) => _rubyClasses.FirstOrDefault(type => 
		{
			RubySerializableClassAttribute attribute = (RubySerializableClassAttribute)type.GetCustomAttribute(typeof(RubySerializableClassAttribute));
			return attribute?.ClassName == rubyName;
		});
		*/
	}

	#region Attributes
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class RubySerializableAttribute(string RubyVariableName) : Attribute
	{
		public string VariableName = RubyVariableName;
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class RubySerializableClassAttribute(string RubyClassName) : Attribute
	{
		public string ClassName = RubyClassName;
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class RubyValueDependedSubClassAttribute(string key, object value) : Attribute
	{
		public string Key = key;
		public object Value = value;
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class RubyParameterDependedSubClassAttribute(string parametersKey, int valueId, object value) : Attribute
	{
		public string ParametersKey = parametersKey;
		public int ID = valueId;
		public object Value = value;
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class EventParameterAttribute(int ParameterID) : Attribute
	{
		public int ID = ParameterID;
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ConditionalBranchAttribute(int ConditionalType) : Attribute
	{
		public int Type = ConditionalType;
	}
	#endregion
}
