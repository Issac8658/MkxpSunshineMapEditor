@tool
extends Node

@export var rxdata_path : String
@export_tool_button("Parse")
var parse_func = load_data

func load_data():
	load_rxdata(rxdata_path)


var symbol_cache: Array = []
var object_cache: Array = []

func load_rxdata(path: String):
	symbol_cache.clear()
	object_cache.clear()
	
	var file = FileAccess.open(path, FileAccess.READ)
	if not file:
		print("Не удалось открыть файл: ", path)
		return

	var major = file.get_8()
	var minor = file.get_8()
	if major != 4 or minor != 8:
		print("Неверная версия Marshal: ", major, ".", minor)
		return

	print("--- Начало парсинга ---")
	var data = parse_marshal_element(file)
	print("--- Парсинг успешно завершен ---")
	print("Результат: ", data)

func parse_marshal_element(file: FileAccess):
	if file.get_position() >= file.get_length():
		return null
		
	var type_flag = char(file.get_8())
	
	match type_flag:
		'0': # nil
			return null
		'T': # true
			return true
		'F': # false
			return false
			
		'i': # Число (Fixnum)
			return parse_marshal_fixnum(file)
			
		':': # Создание нового Символа (Symbol)
			var length = parse_marshal_fixnum(file)
			var sym = file.get_buffer(length).get_string_from_utf8()
			symbol_cache.append(sym) # Сохраняем в кэш
			return sym
			
		';': # Ссылка на уже существующий Символ (Symlink)
			var index = parse_marshal_fixnum(file)
			if index >= 0 and index < symbol_cache.size():
				return symbol_cache[index]
			return "unknown_sym_" + str(index)
			
		'"': # Строка (String)
			var length = parse_marshal_fixnum(file)
			var str_val = file.get_buffer(length).get_string_from_utf8()
			object_cache.append(str_val)
			return str_val
			
		'{': # Обычный Хэш (Словарь)
			var size = parse_marshal_fixnum(file)
			
			# ЗАЩИТА ОТ ЗАВИСАНИЯ: Если размер неадекватный, прерываемся
			if size < 0 or size > 10000:
				print("КРИТИЧЕСКАЯ ОШИБКА: Неверный размер хэша (", size, "). Парсер сбит с позиции.")
				return {}
				
			var dictionary = {}
			object_cache.append(dictionary) # Хэш регистрируется в кэше объектов
			
			for i in range(size):
				var key = parse_marshal_element(file)
				var value = parse_marshal_element(file)
				if key != null:
					dictionary[key] = value
			return dictionary
			
		'o': # Объект (Например, RPG::MapInfo)
			var class_symbol = parse_marshal_element(file) # Читаем имя класса (это символ)
			var num_vars = parse_marshal_fixnum(file)       # Количество переменных инстанса
			
			if num_vars < 0 or num_vars > 100:
				print("КРИТИЧЕСКАЯ ОШИБКА: Неверное кол-во свойств объекта (", num_vars, ")")
				return {}
				
			var obj_data = { "__class__": class_symbol }
			object_cache.append(obj_data) # Объект регистрируется в кэше
			
			for i in range(num_vars):
				var var_name = parse_marshal_element(file)  # Например, "@name"
				var var_value = parse_marshal_element(file) # Значение свойства
				if var_name != null:
					obj_data[var_name] = var_value
			return obj_data
			
		'I': # Объект со скрытыми свойствами (IVAR)
			# Оборачивает строки или объекты, добавляя им метаданные (например, кодировку)
			var internal_element = parse_marshal_element(file)
			var num_ivars = parse_marshal_fixnum(file)
			
			if num_ivars < 0 or num_ivars > 50:
				return internal_element
				
			for i in range(num_ivars):
				var _ivar_name = parse_marshal_element(file)   # Пропускаем метаданные вроде кодировки
				var _ivar_value = parse_marshal_element(file)
			return internal_element
			
		'@': # Ссылка на уже созданный объект (Object link)
			var index = parse_marshal_fixnum(file)
			if index >= 0 and index < object_cache.size():
				return object_cache[index]
			return null
			
		'[': # Массив (Array)
			var size = parse_marshal_fixnum(file)
			if size < 0 or size > 5000:
				print("КРИТИЧЕСКАЯ ОШИБКА: Неверный размер массива (", size, ")")
				return []
				
			var array = []
			object_cache.append(array) # Массив регистрируется в кэше
			
			for i in range(size):
				array.append(parse_marshal_element(file))
			return array
			
		'u': # Пользовательский класс (Userdata - Table, Color, Tone)
			var class_symbol = parse_marshal_element(file) # Название класса (например, "Table")
			var binary_data_len = parse_marshal_fixnum(file) # Длина бинарных данных
			var buffer = file.get_buffer(binary_data_len)
			
			var user_object = { "__class__": class_symbol, "__is_userdata__": true }
			object_cache.append(user_object)
			
			# Разбираем бинарную структуру в зависимости от класса
			if class_symbol == "Table":
				user_object["table_data"] = parse_rmxp_table(buffer)
			elif class_symbol == "Color":
				user_object["color"] = parse_rmxp_color(buffer)
			elif class_symbol == "Tone":
				user_object["tone"] = parse_rmxp_tone(buffer)
			else:
				user_object["raw_bytes"] = buffer # Сохраняем как есть, если класс неизвестен
				
			return user_object

		_ :
			print("Встречен неизвестный тег Marshal в позиции ", file.get_position() - 1, ": ", type_flag)
			return null

func parse_marshal_fixnum(file: FileAccess) -> int:
	var c = file.get_8()
	if c >= 128:
		c -= 256 # Конвертируем в знаковый int8 вручную
		
	if c == 0: return 0
	if c > 5:  return c - 5
	if c < -5: return c + 5
	
	if c > 0 and c <= 4:
		var value = 0
		for i in range(c):
			value |= (file.get_8() << (8 * i))
		return value
		
	if c < 0 and c >= -4:
		var value = -1
		var bytes_count = -c
		for i in range(bytes_count):
			var mask = ~(0xFF << (8 * i))
			value = (value & mask) | (file.get_8() << (8 * i))
		return value
	return 0
	
func parse_rmxp_table(buffer: PackedByteArray) -> Dictionary:
	var stream = StreamPeerBuffer.new()
	stream.data_array = buffer
	
	if buffer.size() < 20: return {}
	
	var dimensions = stream.get_32() # Всегда 3 для карт RMXP
	var width = stream.get_32()      # Ширина карты
	var height = stream.get_32()     # Высота карты
	var layers = stream.get_32()     # Слои (обычно 3)
	var total_tiles = stream.get_32() # Общее число тайлов (W * H * L)
	
	var map_grid = []
	# Создаем структуру трехмерного массива [layer][y][x]
	for z in range(layers):
		var layer_grid = []
		for y in range(height):
			var row = []
			for x in range(width):
				var tile_id = stream.get_16() # ID тайла (2 байта)
				row.append(tile_id)
			layer_grid.append(row)
		map_grid.append(layer_grid)
		
	return {
		"width": width,
		"height": height,
		"layers": layers,
		"grid": map_grid
	}

func parse_rmxp_color(buffer: PackedByteArray) -> Color:
	var stream = StreamPeerBuffer.new()
	stream.data_array = buffer
	if buffer.size() < 32: return Color.WHITE
	# В RGSS Color хранится как 4 числа double (по 8 байт каждое)
	var r = stream.get_double()
	var g = stream.get_double()
	var b = stream.get_double()
	var a = stream.get_double()
	return Color(r / 255.0, g / 255.0, b / 255.0, a / 255.0)

func parse_rmxp_tone(buffer: PackedByteArray) -> Dictionary:
	var stream = StreamPeerBuffer.new()
	stream.data_array = buffer
	if buffer.size() < 32: return {}
	# Тон экрана (RGSS Tone): Red, Green, Blue, Gray (4 double)
	return {
		"r": stream.get_double(),
		"g": stream.get_double(),
		"b": stream.get_double(),
		"gray": stream.get_double()
	}
