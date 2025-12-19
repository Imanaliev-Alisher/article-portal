using ContentPortal.Models;
using Microsoft.AspNetCore.Identity;

namespace ContentPortal.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(PortalContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Проверяем, есть ли уже данные
        if (context.Articles.Any())
        {
            Console.WriteLine("База данных уже содержит данные. Пропускаем инициализацию.");
            return;
        }

        Console.WriteLine("Создание тестовых данных...");

        // Создаем роли
        var roles = new[] { "Admin", "Editor", "User" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        Console.WriteLine($"✓ Создано {roles.Length} ролей");

        // Создаем тестовых пользователей
        var users = new List<ApplicationUser>();
        
        var user1 = new ApplicationUser
        {
            UserName = "admin@portal.com",
            Email = "admin@portal.com",
            FullName = "Администратор",
            EmailConfirmed = true,
            RegisteredAt = DateTime.UtcNow.AddMonths(-6)
        };
        await userManager.CreateAsync(user1, "Admin123!");
        await userManager.AddToRoleAsync(user1, "Admin");
        users.Add(user1);

        var user2 = new ApplicationUser
        {
            UserName = "editor@portal.com",
            Email = "editor@portal.com",
            FullName = "Редактор Иван",
            EmailConfirmed = true,
            RegisteredAt = DateTime.UtcNow.AddMonths(-3)
        };
        await userManager.CreateAsync(user2, "Editor123!");
        await userManager.AddToRoleAsync(user2, "Editor");
        users.Add(user2);

        var user3 = new ApplicationUser
        {
            UserName = "user@portal.com",
            Email = "user@portal.com",
            FullName = "Мария Сидорова",
            EmailConfirmed = true,
            RegisteredAt = DateTime.UtcNow.AddMonths(-1)
        };
        await userManager.CreateAsync(user3, "User123!");
        await userManager.AddToRoleAsync(user3, "User");
        users.Add(user3);

        Console.WriteLine($"✓ Создано {users.Count} пользователей");

        // Создаем категории
        var categories = new List<Category>
        {
            new Category 
            { 
                Name = "Технологии", 
                Description = "Новости и статьи о технологиях и IT" 
            },
            new Category 
            { 
                Name = "Наука", 
                Description = "Научные открытия и исследования" 
            },
            new Category 
            { 
                Name = "Образование", 
                Description = "Статьи об образовании и обучении" 
            },
            new Category 
            { 
                Name = "Программирование", 
                Description = "Разработка ПО, языки программирования, фреймворки" 
            },
            new Category 
            { 
                Name = "Искусственный интеллект", 
                Description = "ИИ, машинное обучение, нейронные сети" 
            },
            new Category 
            { 
                Name = "Веб-разработка", 
                Description = "Frontend, Backend, Full-stack разработка" 
            }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Создано {categories.Count} категорий");

        // Создаем статьи
        var random = new Random();
        var articles = new List<Article>
        {
            new Article
            {
                Title = "Введение в ASP.NET Core",
                Content = @"ASP.NET Core - это кросс-платформенный фреймворк с открытым исходным кодом для создания современных веб-приложений.

Основные преимущества:
- Высокая производительность
- Кросс-платформенность (Windows, Linux, macOS)
- Модульная архитектура
- Встроенная поддержка Dependency Injection
- Поддержка современных паттернов разработки

ASP.NET Core идеально подходит для создания веб-приложений, REST API, микросервисов и других веб-решений.",
                CategoryId = categories[3].Id,
                UserId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Article
            {
                Title = "Entity Framework Core: руководство для начинающих",
                Content = @"Entity Framework Core (EF Core) - это современная объектно-реляционная библиотека (ORM) для .NET.

Что такое ORM?
ORM позволяет работать с базой данных, используя объекты C# вместо SQL-запросов.

Основные возможности EF Core:
1. LINQ запросы для работы с данными
2. Отслеживание изменений (Change Tracking)
3. Миграции для управления схемой БД
4. Поддержка множества СУБД (SQL Server, PostgreSQL, SQLite, MySQL)
5. Асинхронные операции

Пример использования:
var articles = await context.Articles
    .Include(a => a.Category)
    .Where(a => a.PublishedAt > DateTime.Now.AddDays(-7))
    .ToListAsync();",
                CategoryId = categories[3].Id,
                UserId = users[1].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Article
            {
                Title = "Что такое машинное обучение?",
                Content = @"Машинное обучение (Machine Learning, ML) - это раздел искусственного интеллекта, который позволяет компьютерам обучаться на данных без явного программирования.

Типы машинного обучения:

1. Обучение с учителем (Supervised Learning)
   - Классификация
   - Регрессия

2. Обучение без учителя (Unsupervised Learning)
   - Кластеризация
   - Уменьшение размерности

3. Обучение с подкреплением (Reinforcement Learning)
   - Агент учится принимать решения

Популярные библиотеки:
- TensorFlow
- PyTorch
- Scikit-learn
- Keras

Применение: распознавание образов, рекомендательные системы, прогнозирование, обработка естественного языка.",
                CategoryId = categories[4].Id,
                UserId = users[2].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Article
            {
                Title = "REST API: основные принципы",
                Content = @"REST (Representational State Transfer) - это архитектурный стиль для разработки веб-сервисов.

Основные принципы REST:

1. Клиент-серверная архитектура
2. Отсутствие состояния (Stateless)
3. Кэширование
4. Единообразие интерфейса
5. Слоистая система

HTTP методы:
- GET - получение данных
- POST - создание ресурса
- PUT - полное обновление
- PATCH - частичное обновление
- DELETE - удаление

Коды ответов:
- 200 OK - успешно
- 201 Created - создано
- 400 Bad Request - неверный запрос
- 401 Unauthorized - не авторизован
- 404 Not Found - не найдено
- 500 Internal Server Error - ошибка сервера

RESTful API должен быть предсказуемым, понятным и легким в использовании.",
                CategoryId = categories[5].Id,
                UserId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Article
            {
                Title = "Паттерны проектирования в C#",
                Content = @"Паттерны проектирования - это типовые решения часто встречающихся проблем в разработке ПО.

Основные категории паттернов:

1. Порождающие (Creational)
   - Singleton - единственный экземпляр класса
   - Factory - создание объектов через фабрику
   - Builder - пошаговое конструирование объектов

2. Структурные (Structural)
   - Adapter - адаптация интерфейса
   - Decorator - динамическое добавление функциональности
   - Facade - упрощенный интерфейс

3. Поведенческие (Behavioral)
   - Strategy - выбор алгоритма в runtime
   - Observer - подписка на события
   - Command - инкапсуляция запроса

Зачем нужны паттерны?
- Повторное использование решений
- Улучшение читаемости кода
- Упрощение поддержки
- Общий язык для разработчиков",
                CategoryId = categories[3].Id,
                UserId = users[1].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Article
            {
                Title = "Docker для разработчиков",
                Content = @"Docker - это платформа для разработки, доставки и запуска приложений в контейнерах.

Что такое контейнер?
Контейнер - это легковесная, изолированная среда выполнения, которая содержит все необходимое для работы приложения.

Преимущества Docker:
- Консистентность окружения
- Быстрый деплой
- Изоляция приложений
- Эффективное использование ресурсов
- Портативность

Основные команды:
docker build - сборка образа
docker run - запуск контейнера
docker ps - список контейнеров
docker images - список образов
docker-compose up - запуск нескольких контейнеров

Dockerfile - это текстовый файл с инструкциями для создания Docker образа.

Docker Compose позволяет определить и запустить многоконтейнерные приложения.",
                CategoryId = categories[0].Id,
                UserId = users[2].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Article
            {
                Title = "Основы нейронных сетей",
                Content = @"Нейронная сеть - это вычислительная модель, вдохновленная работой человеческого мозга.

Структура нейронной сети:

1. Входной слой (Input Layer)
   - Получает исходные данные

2. Скрытые слои (Hidden Layers)
   - Обрабатывают информацию
   - Извлекают признаки

3. Выходной слой (Output Layer)
   - Выдает результат

Процесс обучения:
1. Прямое распространение (Forward Propagation)
2. Вычисление ошибки
3. Обратное распространение (Backpropagation)
4. Обновление весов

Функции активации:
- ReLU (Rectified Linear Unit)
- Sigmoid
- Tanh
- Softmax (для классификации)

Популярные архитектуры:
- CNN (Convolutional Neural Networks) - для изображений
- RNN (Recurrent Neural Networks) - для последовательностей
- Transformer - для NLP задач",
                CategoryId = categories[4].Id,
                UserId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Article
            {
                Title = "Git и GitHub: контроль версий",
                Content = @"Git - это распределенная система контроля версий, которая отслеживает изменения в файлах.

Основные концепции:

Repository (Репозиторий)
- Хранилище истории проекта

Commit (Коммит)
- Снимок состояния проекта

Branch (Ветка)
- Независимая линия разработки

Базовые команды Git:

git init - инициализация репозитория
git add - добавление файлов в staging
git commit - создание коммита
git push - отправка изменений на сервер
git pull - получение изменений с сервера
git branch - работа с ветками
git merge - слияние веток
git checkout - переключение веток

Workflow:
1. Создать ветку (feature branch)
2. Внести изменения
3. Закоммитить
4. Создать Pull Request
5. Code Review
6. Merge в main

GitHub - это платформа для хостинга Git-репозиториев с дополнительными инструментами для совместной разработки.",
                CategoryId = categories[3].Id,
                UserId = users[1].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Article
            {
                Title = "Микросервисная архитектура",
                Content = @"Микросервисная архитектура - это подход к разработке приложения как набора небольших независимых сервисов.

Характеристики микросервисов:

1. Независимое развертывание
   - Каждый сервис можно обновлять отдельно

2. Узкая специализация
   - Один сервис = одна бизнес-функция

3. Децентрализация
   - Каждый сервис может иметь свою БД

4. Отказоустойчивость
   - Сбой одного сервиса не роняет всю систему

Преимущества:
- Гибкость технологий
- Масштабируемость
- Независимые команды
- Быстрая доставка изменений

Недостатки:
- Сложность инфраструктуры
- Распределенные транзакции
- Мониторинг и отладка
- Сетевые задержки

Паттерны микросервисов:
- API Gateway
- Service Discovery
- Circuit Breaker
- Event Sourcing
- CQRS

Инструменты: Docker, Kubernetes, RabbitMQ, Kafka, Consul.",
                CategoryId = categories[0].Id,
                UserId = users[2].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Article
            {
                Title = "Тестирование ПО: виды и подходы",
                Content = @"Тестирование - это процесс проверки соответствия ПО требованиям и выявления дефектов.

Уровни тестирования:

1. Unit-тесты (Модульные)
   - Тестирование отдельных методов/функций
   - Быстрые и изолированные

2. Integration-тесты (Интеграционные)
   - Тестирование взаимодействия компонентов

3. End-to-End тесты (E2E)
   - Тестирование всей системы целиком

Виды тестирования:

Функциональное:
- Проверка функций приложения

Нефункциональное:
- Производительность
- Безопасность
- Юзабилити
- Совместимость

Подходы:

TDD (Test-Driven Development)
1. Написать тест
2. Написать код
3. Рефакторинг

BDD (Behavior-Driven Development)
- Тесты описывают поведение

Инструменты для .NET:
- xUnit, NUnit, MSTest
- Moq (для mock-объектов)
- FluentAssertions
- Selenium (для UI тестов)

Хорошее покрытие тестами = качественный код!",
                CategoryId = categories[3].Id,
                UserId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Создано {articles.Count} статей");

        Console.WriteLine("\n=== Тестовые данные успешно созданы! ===\n");
        Console.WriteLine("Тестовые пользователи:");
        Console.WriteLine("1. Email: admin@portal.com   | Пароль: Admin123!  | Роль: Admin");
        Console.WriteLine("2. Email: editor@portal.com  | Пароль: Editor123! | Роль: Editor");
        Console.WriteLine("3. Email: user@portal.com    | Пароль: User123!   | Роль: User");
        Console.WriteLine("\nАдминистратор и редактор имеют доступ к панели администрирования.\n");
    }
}
