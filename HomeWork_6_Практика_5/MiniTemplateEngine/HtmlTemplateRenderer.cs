using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MiniTemplateEngine
{
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            string result = RenderFromFile(inputFilePath, dataModel);
            File.WriteAllText(outputFilePath, result);
            return result;
        }
        public string RenderFromFile(string filePath, object dataModel)
        {
            if (File.Exists(filePath) && IsAcceptableModel(dataModel))
            {
                try
                {
                    string htmlContent = File.ReadAllText(filePath);
                    return RenderFromString(htmlContent, dataModel);
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Файл index.html не найден!");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Нет доступа к файлу!");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Ошибка ввода-вывода: {ex.Message}");
                }
            }
            Console.WriteLine("Отсутствует файл по пути или не верная по типу модель данных");
            return null;
        }
        private static bool IsAcceptableModel(object? model)
        {
            if (model == null) return false;

            // 1) словарь
            if (model is IDictionary<string, object>) return true;

            // 2) отклоняем "простые" типы
            var type = model.GetType();
            if (IsSimpleType(type)) return false;

            // 3) всё остальное считаем "объектом" (объект какого-то класса или анонимный тип)
            return true;
        }
        private static bool IsSimpleType(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime)) return true;

            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) return IsSimpleType(underlying);

            return false;
        }


        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            htmlTemplate = ProcessIfElse(htmlTemplate, dataModel);
            htmlTemplate = ProcessForeach(htmlTemplate, dataModel);
            htmlTemplate = ReplaceVariables(htmlTemplate, dataModel);
            return htmlTemplate;

        }

        /// Обрабатывает условные блоки
        /// $if(condition) ... $else ... $endif
        private string ProcessIfElse(string input, object model)
        {
            //ValidateIfBlocks(input);

            var regex = new Regex(@"\$if\((.*?)\)([\s\S]*?)(?:\$else([\s\S]*?))?\$endif");

            int ifindex = input.IndexOf("$if", StringComparison.Ordinal);

            int forindex = input.IndexOf("$foreach", StringComparison.Ordinal);

            if (ifindex > forindex && forindex != -1)
                return input;

            var output = regex.Replace(input, match =>
            {
                string condition = match.Groups[1].Value.Trim();
                string ifBody = match.Groups[2].Value;
                string elseBody = match.Groups[3].Value;

                bool result = EvaluateCondition(condition, model);

                
                ///Если у нас то что внутри if true то тогда она рекурсивно проверяет
                ///есть ли в body ещё вложенные if или foreach
                if (result)
                {
                    return RenderFromString(ifBody, model);
                }
                if(!string.IsNullOrEmpty(elseBody))
                {
                    return RenderFromString(elseBody, model);
                }
                return string.Empty;
            });
            return regex.IsMatch(output)  ? RenderFromString(output,model) : output;
        }

        private void ValidateIfBlocks(string input)
        {
            int depth = 0;
            int index = 0;

            while (index < input.Length)
            {
                int ifPos = input.IndexOf("$if", index, StringComparison.Ordinal);
                int endPos = input.IndexOf("$endif", index, StringComparison.Ordinal);

                if (ifPos == -1 && endPos == -1)
                    break;

                // Если нашли $if раньше
                if (ifPos != -1 && (ifPos < endPos || endPos == -1))
                {
                    depth++;
                    index = ifPos + 3; // длина "$if"
                }
                else
                {
                    // нашли $endif
                    depth--;
                    if (depth < 0)
                        throw new InvalidOperationException("Обнаружен $endif без соответствующего $if.");

                    index = endPos + 6; // длина "$endif"
                }
            }

            if (depth > 0)
                throw new InvalidOperationException("Обнаружен незакрытый $if: отсутствует $endif.");
        }


        /// Обрабатывает циклы по коллекции
        /// $foreach(var itemName in collectionPath) ... $endfor
        public string ProcessForeach(string input, object model)
        {
            const string startToken = "$foreach(";
            const string endToken = "$endfor";

            int startIndex = input.IndexOf(startToken, StringComparison.Ordinal);
            while (startIndex != -1)
            {
                int headerStart = startIndex + startToken.Length;
                int headerEnd = input.IndexOf(')', headerStart);
                if (headerEnd == -1) throw new InvalidOperationException($"Некорректный заголовок foreach, не хватает скобки справа");

                string header = input.Substring(headerStart, headerEnd - headerStart).Trim();
                var m = Regex.Match(header, @"^\s*var\s+(\w+)\s+in\s+(.+)$");
                if (!m.Success)
                {
                    /// Здесь проверяется без var 
                    m = Regex.Match(header, @"^\s*(\w+)\s+in\s+(.+)$");
                    if (!m.Success) 
                        throw new InvalidOperationException($"Некорректный заголовок foreach: '{header}'");
                }
                string itemName = m.Groups[1].Value;
                string collectionExpr = m.Groups[2].Value.Trim();

                int bodyStart = headerEnd + 1;
                int pos = bodyStart;
                int depth = 1;
                int foundEnd = -1;

                while (pos < input.Length)
                {
                    int nextFor = input.IndexOf(startToken, pos, StringComparison.Ordinal);
                    int nextEndFor = input.IndexOf(endToken, pos, StringComparison.Ordinal);

                    if (nextEndFor == -1) throw new InvalidOperationException($"Обнаружен незакрытый блок $foreach, начатый на позиции {startIndex}. Ожидался $foreach.");
                    if (nextFor != -1 && nextFor < nextEndFor)
                    {
                        depth++;
                        pos = nextFor + startToken.Length;
                        continue;
                    }
                    else
                    {
                        depth--;
                        pos = nextEndFor + endToken.Length;
                        if (depth == 0)
                        {
                            foundEnd = nextEndFor;
                            break;
                        }
                    }
                }
                if (foundEnd == -1) throw new InvalidOperationException("Не найден $endfor для $foreach");
                string body = input.Substring(bodyStart, foundEnd - bodyStart);
                var collectionObj = ParseValue(collectionExpr, model);
                if (collectionObj == null)
                {
                    // если коллекции нет — удаляем блок (ничего не рендерим)
                    input = input.Substring(0, startIndex) + "" + input.Substring(foundEnd + endToken.Length);
                    startIndex = input.IndexOf(startToken, startIndex, StringComparison.Ordinal);
                    continue;
                }
                if (!(collectionObj is System.Collections.IEnumerable collection) || collectionObj is string)
                {
                    throw new InvalidOperationException($"Выражение '{collectionExpr}' не является коллекцией");
                }
                var sb = new StringBuilder();
                foreach (var item in collection)
                {
                    var scoped = new ScopedModel(model);
                    scoped.SetLocal(itemName, item);

                    string rendered = RenderFromString(body, scoped);
                    sb.Append(rendered);
                }
                input = input.Substring(0, startIndex) + sb.ToString() + input.Substring(foundEnd + endToken.Length);

                startIndex = input.IndexOf(startToken, startIndex + sb.Length, StringComparison.Ordinal);
            }
            if ((input.IndexOf("$if") != -1 && input.IndexOf("$endif") != -1) || input.IndexOf("$foreach") != -1)
                return RenderFromString(input, model);
            return input;
        }

        /// Заменяет все вхождения переменных вида ${path.to.property} на их значения из модели.
        private static string ReplaceVariables(string input, object model)
        {
            var regex = new Regex(@"\$\{([\w\.]+)\}");
            return regex.Replace(input, match =>
            {
                string path = match.Groups[1].Value;
                var value = GetPropertyValue(model, path);
                if (value == path)
                    return $"${{{path}}}";
                return value?.ToString() ?? $"{input}";
            });
        }

        /// Вычисляет результат условного выражения внутри if()
        private bool EvaluateCondition(string expression, object model)
        {
            // Убираем пробелы
            expression = expression.Trim();

            // Проверяем на выражение сравнения
            var comparisonRegex = new Regex(@"^([\w\.]+)\s*(==|!=|>=|<=|>|<)(?![=])\s*(.+)$");
            var match = comparisonRegex.Match(expression);

            if (match.Success)
            {
                var leftExpr = match.Groups[1].Value.Trim();
                var op = match.Groups[2].Value;
                var rightExpr = match.Groups[3].Value.Trim();

                var leftValue = ParseValue(leftExpr, model);
                var rightValue = ParseValue(rightExpr, model);

                int compareResult = CompareValues(leftValue, rightValue);

                return op switch
                {
                    "==" => compareResult == 0,
                    "!=" => compareResult != 0,
                    ">" => compareResult > 0,
                    "<" => compareResult < 0,
                    ">=" => compareResult >= 0,
                    "<=" => compareResult <= 0,
                    _ => throw new InvalidOperationException($"Неизвестный оператор {op}")
                };
            }

            // Если это не сравнение — ожидаем просто bool-свойство
            var value = GetPropertyValue(model, expression);
            if (value is bool b) return b;

            throw new InvalidOperationException(
                $"Выражение '{expression}' не является булевым значением и не поддерживает сравнение."
            );
        }

        /// Парсит значение выражения справа или слева от оператора.
        private object? ParseValue(string raw, object model)
        {
            // если это строка в кавычках
            if (raw.StartsWith("\"") && raw.EndsWith("\""))
                return raw.Substring(1, raw.Length - 2);

            if (raw == "true")
                return true;
            if (raw == "false")
                return false;

            // если это число
            if (int.TryParse(raw, out int intVal))
                return intVal;
            if (double.TryParse(raw, out double doubleVal))
                return doubleVal;

            // иначе, возможно это свойство модели
            return GetPropertyValue(model, raw);
        }
        /// Сравнивает два значения
        private int CompareValues(object? left, object? right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            if (left is IComparable c1 && right is IComparable c2)
                return c1.CompareTo(Convert.ChangeType(right, left.GetType()));

            return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }

        /// Получает значение по пути свойства
        public static object? GetPropertyValue(object? model, string path)
        {
            if (model == null) return null;
            var parts = path.Split('.');

            object? current = model;

            foreach (var part in parts)
            {
                if (current == null) return null;

                // 1. Если словарь — достаём по ключу
                if (current is IDictionary<string, object> dict && dict.TryGetValue(part, out var val))
                {
                    current = val;
                    continue;
                }

                // 2. Если объект — достаём свойство
                var type = current.GetType();
                var prop = type.GetProperty(part);
                if (prop != null)
                {
                    current = prop.GetValue(current);
                    continue;
                }

                // 3. Если поле
                var field = type.GetField(part);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }

                if(current is IEnumerable<object> massiv)
                {
                    foreach(var elements in massiv)
                    {
                        current = GetPropertyValue(elements, part);
                        continue;
                    }
                }

                // Если ничего не нашли
                return path;
            }

            return current;
        }
    }
}