using MiniTemplateEngine;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MiniTemplateEngineUnitTests
{
    [TestClass]
    public sealed class HtmlTemplateRendererTests
    {
        [TestMethod]
        public void RenderFromString_When_Return()
        {
            //Arrange
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name}</h1>";
            var model = new { Name = "Тимерхан" };
            string expectedString = "<h1> Привет, Тимерхан</h1>";
            //Act
            var result = testee.RenderFromString(templateHtml, model);
            //Assert
            Assert.AreEqual(expectedString, result);
        }
        [TestMethod]
        public void RenderFromString_WhenDouble_ReturnCorrectString()
        {
            //Arrange
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name}</h1> <p>Привет, ${Name}</p>";
            var model = new { Name = "Тимерхан"};
            string expectedString = "<h1> Привет, Тимерхан</h1> <p>Привет, Тимерхан</p>";
            //Act
            var result = testee.RenderFromString(templateHtml, model);
            //Assert
            Assert.AreEqual(expectedString, result);
        }
        [TestMethod]
        public void RenderFromString_WhenTwoProperties_ReturnCorrectString()
        {
            //Arrange
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name}</h1><p> Привет, ${Email}</p>";
            var model = new { Name = "Тимерхан", Email = "test@test.ru" };
            string expectedString = "<h1> Привет, Тимерхан</h1><p> Привет, test@test.ru</p>";
            //Act
            var result = testee.RenderFromString(templateHtml, model);
            //Assert
            Assert.AreEqual(expectedString, result);
        }
        [TestMethod]
        public void RenderFromString_WhenSubProperties_ReturnCorrectString()
        {
            //Arrange
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name}</h1><p>Группа: ${Group.Name} Привет, ${Email}</p>";
            var model = new { Name = "Тимерхан", Email = "test@test.ru", Group = new { Id = 1, Name = "11-409" } };
            string expectedString = "<h1> Привет, Тимерхан</h1><p>Группа: 11-409 Привет, test@test.ru</p>";
            //Act
            var result = testee.RenderFromString(templateHtml, model);
            //Assert
            Assert.AreEqual(expectedString, result);
        }
        [TestMethod]
        public void RenderFromString_WhenIf_ReturnCorrectString()
        {
            //Arrange
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1>$if(Name == Тимерхан)<p>Привет, Тимерхан</p>$else $endif</h1>";
            var model = new { Name = "Тимерхан", Email = "test@test.ru", Group = new { Id = 1, Name = "11-409" } };
            string expectedString = "<h1><p>Привет, Тимерхан</p></h1>";
            //Act
            var result = testee.RenderFromString(templateHtml, model);
            //Assert
            Assert.AreEqual(expectedString, result);
        }

        //Мною написанные тесты
        
        //1
        [TestMethod]
        public void RenderFromString_WhenForeach_ReturnCorrectString()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
                <h1>$foreach(var item in collection)
                <li>Элемент из коллекции: ${item}</li>$endfor
                </h1>";
            object modelka = new { collection = new[] { "str", "int", "dubl"} };
            string expected = @"
                <h1>
                <li>Элемент из коллекции: str</li>
                <li>Элемент из коллекции: int</li>
                <li>Элемент из коллекции: dubl</li>
                </h1>";
            string result = testee.RenderFromString(templateHTML, modelka);
            Assert.AreEqual(expected, result);
        }

        //2
        [TestMethod]
        public void RenderFromString_WhenNesting_from_if_and_ReplaceVariables()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
            <h1>
            $if(Name == Tony)
                $if(Surname == Montana) 
                    <p>Меня зовут ${Name} ${Surname}</p> $endif<p>${Name}</p>$endif
            </h1>";
            object model = new { Name = "Tony", Surname = "Montana" };
            string exp = @"<h1> <p>Меня зовут Tony Montana</p> <p>Tony</p> </h1>";
            string result = testee.RenderFromString(templateHTML, model);
            Assert.AreEqual(Regex.Replace(exp, @"\s+", ""), Regex.Replace(result, @"\s+", ""));
        }

        //3
        [TestMethod]
        public void RenderFromString_WhenForeach_Nested()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
                        <h1>
                        $foreach(var univer in univers) 
                            $foreach(var faculty in univer.Institutes)
                                <li>Университет:${univer.Name}, факультет:${faculty}</li>$endfor $endfor
                        </h1>";
            var univers = new { univers = new[] { new { Name = "KFU", Institutes = new[] { "ITIS", "IVMIT", "MEHMAT" } }, new { Name = "KNITU", Institutes = new[] { "ИУАИТ", "ИТЛПМД", "ИХТИ" } } } };
            string expected = @"
                        <h1>
                            <li>Университет:KFU, факультет:ITIS</li>
                            <li>Университет:KFU, факультет:IVMIT</li>
                            <li>Университет:KFU, факультет:MEHMAT</li>
                            <li>Университет:KNITU, факультет:ИУАИТ</li>
                            <li>Университет:KNITU, факультет:ИТЛПМД</li>
                            <li>Университет:KNITU, факультет:ИХТИ</li>
                        </h1>";
            string result = testee.RenderFromString(templateHTML, univers);
            Assert.AreEqual(
                Regex.Replace(expected, @"\s+", ""),
                Regex.Replace(result, @"\s+", "")
                );
        }

        //4
        [TestMethod]
        public void RenderFromString_When_a_foreach_is_nested_in_the_if()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
            <h1>$if(5 == 5) $foreach(var univer in univers) $foreach(var faculty in univer.Institutes)
            <li>Университет:${univer.Name}, факультет:${faculty}</li>$endfor $endfor $endif
            </h1>";
            var univers = new { univers = new[] { new { Name = "KFU", Institutes = new[] { "ITIS", "IVMIT", "MEHMAT" } }, new { Name = "KNITU", Institutes = new[] { "ИУАИТ", "ИТЛПМД", "ИХТИ" } } } };
            string expected = @"
            <h1>
            <li>Университет:KFU, факультет:ITIS</li>
            <li>Университет:KFU, факультет:IVMIT</li>
            <li>Университет:KFU, факультет:MEHMAT</li>
            <li>Университет:KNITU, факультет:ИУАИТ</li>
            <li>Университет:KNITU, факультет:ИТЛПМД</li>
            <li>Университет:KNITU, факультет:ИХТИ</li>
            </h1>";
            string result = testee.RenderFromString(templateHTML, univers);
            Assert.AreEqual(
                Regex.Replace(expected, @"\s+", ""),
                Regex.Replace(result, @"\s+", "")
                );
        }

        //5
        [TestMethod]
        public void RenderFromString_When_if_has_a_foreach_nested_and_in_else_has_a_foreach_nested()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
            <h1>$if(5 == 6)
                $foreach(var univer in univers)
                    $foreach(var faculty in univer.Institutes)
                        <li>Университет:${univer.Name}, факультет:${faculty}</li> $endfor $endfor
            $else
                $foreach(var univer in univers)
                    $foreach(var faculty in univer.Institutes)
                        <li>Университет вот такой вот жиесть:${univer.Name}, факультет тоже такой вот жиесть:${faculty}</li>$endfor $endfor $endif
            </h1>";
            var univers = new { univers = new[] { new { Name = "KFU", Institutes = new[] { "ITIS", "IVMIT", "MEHMAT" } }, new { Name = "KNITU", Institutes = new[] { "ИУАИТ", "ИТЛПМД", "ИХТИ" } } } };
            string expected = @"
            <h1>
                <li>Университет вот такой вот жиесть:KFU, факультет тоже такой вот жиесть:ITIS</li>
                <li>Университет вот такой вот жиесть:KFU, факультет тоже такой вот жиесть:IVMIT</li>
                <li>Университет вот такой вот жиесть:KFU, факультет тоже такой вот жиесть:MEHMAT</li>
                <li>Университет вот такой вот жиесть:KNITU, факультет тоже такой вот жиесть:ИУАИТ</li>
                <li>Университет вот такой вот жиесть:KNITU, факультет тоже такой вот жиесть:ИТЛПМД</li>
                <li>Университет вот такой вот жиесть:KNITU, факультет тоже такой вот жиесть:ИХТИ</li>
            </h1>";
            string result = testee.RenderFromString(templateHTML, univers);
            Assert.AreEqual(
                Regex.Replace(expected, @"\s+", ""),
                Regex.Replace(result, @"\s+", "")
                );
        }

        //6
        [TestMethod]
        public void RenderFromString_When_if_is_nested_in_foreach()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
            <h1>
            $foreach(var univer in univers)
                $foreach(var faculty in univer.Institutes)
                        $if(univer.Name == KFU)
                            <li>Университет:${univer.Name} лучший, факультет/институт:${faculty}</li> 
                        $else
                            <li>Университет:${univer.Name} и его факультет/институт:${faculty} так себе</li> $endif $endfor $endfor
            </h1>";
            var univers = new { univers = new[] { new { Name = "KFU", Institutes = new[] { "ITIS", "IVMIT", "MEHMAT" } }, new { Name = "KNITU", Institutes = new[] { "ИУАИТ", "ИТЛПМД", "ИХТИ" } } } };
            string expected = @"
            <h1>
                <li>Университет:KFU лучший, факультет/институт:ITIS</li>
                <li>Университет:KFU лучший, факультет/институт:IVMIT</li>
                <li>Университет:KFU лучший, факультет/институт:MEHMAT</li>
                <li>Университет:KNITU и его факультет/институт:ИУАИТ так себе</li>
                <li>Университет:KNITU и его факультет/институт:ИТЛПМД так себе</li>
                <li>Университет:KNITU и его факультет/институт:ИХТИ так себе</li>
            </h1>";
            string result = testee.RenderFromString(templateHTML, univers);
            Assert.AreEqual(Regex.Replace(expected, @"\s+", ""), Regex.Replace(result, @"\s+", ""));
        }

        //7
        [TestMethod]
        public void RenderFromString_When_two_if_have_foreach_nested_and_else_has_foreach_nested()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
            <h1>
            $foreach(var univer in univers)
                $foreach(var faculty in univer.Institutes)
                        $if(univer.Name == KFU)
                            <li>Университет:${univer.Name} лучший, факультет/институт:${faculty}</li> 
                        $else
                            <li>Университет:${univer.Name} и его факультет/институт:${faculty} так себе</li> $endif
                        $if(faculty == ITIS)
                            <li>${faculty} это лучший институт!</li> $endif $endfor $endfor
            </h1>";
            var univers = new { univers = new[] { new { Name = "KFU", Institutes = new[] { "ITIS", "IVMIT", "MEHMAT" } }, new { Name = "KNITU", Institutes = new[] { "ИУАИТ", "ИТЛПМД", "ИХТИ" } } } };
            string expected = @"
            <h1>
                <li>Университет:KFU лучший, факультет/институт:ITIS</li>
                <li>ITIS это лучший институт!</li>
                <li>Университет:KFU лучший, факультет/институт:IVMIT</li>
                <li>Университет:KFU лучший, факультет/институт:MEHMAT</li>
                <li>Университет:KNITU и его факультет/институт:ИУАИТ так себе</li>
                <li>Университет:KNITU и его факультет/институт:ИТЛПМД так себе</li>
                <li>Университет:KNITU и его факультет/институт:ИХТИ так себе</li>
            </h1>";
            string result = testee.RenderFromString(templateHTML, univers);
            Assert.AreEqual(Regex.Replace(expected, @"\s+", ""), Regex.Replace(result, @"\s+", ""));
        }

        //8
        [TestMethod]
        public void RenderFromString_When_Foreach_and_If_Are_Nested_BothWays()
        {
            // Arrange
            var testee = new HtmlTemplateRenderer();

            string templateHTML = @"
                <h1>
                $foreach(var univer in univers)
                    $if(univer.Name == ""KFU"")
                        <p>Университет: ${univer.Name}</p>
                        $foreach(var faculty in univer.Institutes)
                            <li>Факультет: ${faculty}</li>
                        $endfor
                    $else
                        <p>Пропущен университет: ${univer.Name}</p>
                    $endif
                $endfor
                </h1>";

            var model = new { univers = new[] { new { Name = "KFU", Institutes = new[] { "ITIS", "IVMIT" } }, new { Name = "KNITU", Institutes = new[] { "ИУАИТ", "ИТЛПМД" } }}};
            string expected = @"
                <h1>
                <p>Университет: KFU</p>
                <li>Факультет: ITIS</li>
                <li>Факультет: IVMIT</li>
                <p>Пропущен университет: KNITU</p>
                </h1>";
            string result = testee.RenderFromString(templateHTML, model);
            string normalize(string s) => Regex.Replace(s, @"\s+", "");
            Assert.AreEqual(normalize(expected), normalize(result));
        }

        //9
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RenderFromString_When_endfor_is_missing()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = @"
                <h1>
                    $foreach(var item in collection)
                        <li>Элемент из коллекции: ${item}</li>
                </h1>";
            var model = new { collection = new[] { "str", "int", "dubl" } };
            
            var result = testee.RenderFromString(templateHtml, model);
        }

        //10
        [TestMethod]      
        public void RenderFromString_When_the_collection_is_null()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = @"
                <h1>
                    $foreach(var item in collection)
                        <li>Элемент из коллекции: ${item}</li>$endfor
                    $if(5 == 5)
                        <p>Пять ровно пяти</p>$endif
                </h1>";
            object? model = null;
            var result = testee.RenderFromString(templateHtml, model!);
            string expected = @"<h1><p>Пять ровно пяти</p></h1>";
            string normalize(string s) => Regex.Replace(s, @"\s+", "");
            Assert.AreEqual(normalize(expected), normalize(result));
        }

        //11
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RenderFromString_When_the_collection_is_string()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = @"
                <h1>
                    $foreach(var item in collection)
                        <li>Элемент из коллекции: ${item} $endfor</li>
                </h1>";
            object? model = "ITIS";
            var result = testee.RenderFromString(templateHtml, model!);
        }

        //12 Надо комментарий с вызова метода ValidateIfBlocks убрать но тогда в одном тесте будет ошибка)
        [TestMethod]
        public void RenderFromString_When_endif_is_missing()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
            <h1>
            $if(Tony == Tony)
                $if(Montana == Montana) 
                    <p>Меня зовут ${Name} ${Surname}</p>$endif
            </h1>";
            object model = new { Name = "Tony", Surname = "Montana" };
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                testee.RenderFromString(templateHTML, model);
            });
        }

        //13
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RenderFromString_When_IfConditionHasInvalidOperator_ShouldThrow()
        {
            var renderer = new HtmlTemplateRenderer();
            string template = @"
                <div>
                    $if(1 === 1)
                        <p>OK</p>
                    $endif
                </div>";

            var model = new { };
            renderer.RenderFromString(template, model);
        }

        //14
        [TestMethod]
        public void RenderFromString_When_IfBlockIsEmpty_ShouldNotBreak()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"<div>$if(1 == 1)$endif</div>";
            object model = new { };
            string result = testee.RenderFromString(templateHTML, model);
            Assert.AreEqual("<div></div>", result);
        }

        //15
        [TestMethod]
        public void RenderFromString_When_StringComparisonWithQuotes_ShouldRenderCorrectly()
        {
            // Arrange
            var testee = new HtmlTemplateRenderer();
            string templateHTML = @"
                <p>
                    $if(Name == ""Tony"")
                        Hello, ${Name}!
                $endif
                </p>";
            object model = new { Name = "Tony" };

            string expected = @"
                <p>
                    Hello, Tony!
                </p>";
            string result = testee.RenderFromString(templateHTML, model);
            string normalize(string s) => Regex.Replace(s, @"\s+", "");
            Assert.AreEqual(normalize(expected), normalize(result));
        }

        //16
        [TestMethod]
        public void RenderFromString_WhenForeachOverIntArray_RendersCorrectly()
        {
            var renderer = new HtmlTemplateRenderer();
            string template = @"
                <ul>
                    $foreach(var num in Numbers)
                        <li>Число: ${num}</li>
                    $endfor
                </ul>
                <p>Всего элементов: ${Numbers.Length}</p>";

            var model = new { Numbers = new int[] { 10, 20, 30, 40 } };

            string expected = @"
                <ul>
                    <li>Число: 10</li>
                    <li>Число: 20</li>
                    <li>Число: 30</li>
                    <li>Число: 40</li>
                </ul>
                <p>Всего элементов: 4</p>";
            string result = renderer.RenderFromString(template, model);

            string normalize(string s) => Regex.Replace(s, @"\s+", "");
            Assert.AreEqual(normalize(expected), normalize(result));
        }

        //17
        [TestMethod]
        public void RenderFromString_WhenForeachOverEmptyIntArray_RendersNothingInside()
        {
            var renderer = new HtmlTemplateRenderer();
            string template = @"
                <div>
                    $foreach(var n in Values)
                        <span>${n}</span>
                    $endfor
                    <p>Конец</p>
                </div>";
            var model = new { Values = new int[0] };
            string expected = @"
                <div>
                    <p>Конец</p>
                </div>";
            string result = renderer.RenderFromString(template, model).Trim();
            string normalize(string s) => Regex.Replace(s, @"\s+", "");
            Assert.AreEqual(normalize(expected), normalize(result));
        }

        //18 Блин надоело долбиться с этим шаблонизатором, каждый тест ошибку нахожу задалбался переделывать его
        [TestMethod]
        public void RenderFromString_WhenIfInsideElseInsideForeach_RendersCorrectly()
        {
            // Arrange
            var renderer = new HtmlTemplateRenderer();

            string template = @"
            <ul>
                $foreach(var person in People)
                    <li>
                        Имя: ${person.Name}
                        $if(person.Age >= 18)
                            — Взрослый
                        $else
                            — Несовершеннолетний
                            $if(person.Age >= 13)
                                (подросток)
                            $else
                                (ребёнок)$endif
                        $endif
                    </li>
                $endfor
            </ul>";

            var model = new
            {
                People = new[]
                {
                new { Name = "Алексей", Age = 25 },
                new { Name = "Марина",  Age = 16 },
                new { Name = "Тимофей", Age = 10 },
                new { Name = "Ольга",   Age = 30 }
            }
            };

            string expected = @"
            <ul>
                
                    <li>
                        Имя: Алексей
                            — Взрослый
                    </li>
                
                    <li>
                        Имя: Марина
                            — Несовершеннолетний
                                (подросток)
                    </li>
                
                    <li>
                        Имя: Тимофей
                            — Несовершеннолетний
                                (ребёнок)
                    </li>
                
                    <li>
                        Имя: Ольга
                            — Взрослый
                    </li>
                
            </ul>";

            // Act
            string result = renderer.RenderFromString(template, model);

            // Assert — нормализуем пробелы и переносы, чтобы тест был устойчивым
            string Normalize(string s) =>
                System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");

            string normalizedResult = Normalize(result);
            string normalizedExpected = Normalize(expected);

            Assert.AreEqual(expected, result);
        }
    }
}