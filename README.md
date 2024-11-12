# Demonstration of ASP.NET Core skills
This is a test assignment I have received from one of the companies I had job interview with. I usually don't upload that kind of stuff to my GitHub, but I figured I should have at least some kind of demo for my backend writing skills.

Important note: I was fairly limited in time I had for this test assignment and there supposed to be additional round of discussions around solution for this assignment, so I took some shortcuts which I would not have used if this was a real life project.

Those include things like hardcoding API url, writing unit tests just for the main logic, not including container support and implementing very basic rest api client.

The lack or simplification of some things does not reflect how I would have actually done things in a real project.

## Test assigment (EN)
The purpose of this test assignment is for candidate to demonstrate their skills in writing scaleable and fault-tolerant services.

We evaluate code structure, used design patterns, correctness and completeness of solution.

### Your task
You need to develop REST service which should calculate the distance between two airports in miles. Airports are supplied as 3 letter IATA code.

Information about the airport is retrieved using the following HTTP request (example for Amsterdam airport):

`GET https://api-url-was-here/airports/AMS HTTP/1.1`

You are allowed to use any third-party components and libraries.

The solution for this task should be implemented in .NET 5 or newer.

## Тестовое задание (RU)
Цель данного задания – продемонстрировать навыки построения масштабируемых и отказоустойчивых сервисов.

Производится оценка структуры кода, используемых шаблонов проектирования, корректности и полноты решения.

### Описание
Разработать REST-сервис для получения расстояния в милях между двумя аэропортами. Аэропорты определяются трёхбуквенным кодом IATA.

Для получения информации об аэропорте используется соответствующий HTTP вызов.

Пример вызова для аэропорта Амстердам (AMS):
`GET https://api-url-was-here/airports/AMS HTTP/1.1`

Допускается использование любых сторонних компонентов и/или библиотек.

Решение должно быть построено на основе .Net core 5.0+
