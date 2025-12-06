
[English version](README.en.md)

# Tasker

Tasker - kanban-подобное веб-приложение для управления задачами и досками. Проект построен как набор сервисов вокруг доменной модели "доски - колонки - карточки", с разделением write/read-моделей и отдельным сервисом аутентификации.

Основной сценарий использования:
- пользователь регистрируется и входит в систему
- создаёт доски (по шаблону или с нуля)
- добавляет колонки и карточки
- назначает исполнителей и метки, управляет дедлайнами
- просматривает доски и карточки через read-модель, оптимизированную под чтение

## Архитектура

Проект организован как monorepo на .NET 8 и React:

- бэкенд на C# с элементами DDD и CQRS
- отдельные сервисы для Auth, BoardWrite и BoardRead
- MySQL как write-хранилище
- Cassandra как хранилище снапшотов read-модели
- Redis для сессий и проверки access токенов
- Kafka-совместимый брокер Redpanda для интеграционных событий
- фронтенд на Vite + React + TypeScript
- мониторинг через Prometheus и Grafana
- Nginx как единая точка входа в docker окружении

## Сервисы и контейнеры

В `deploy/compose/docker-compose.yml` поднимается 11 контейнеров:

### 1. mysql

- Образ: `mysql:8.0`
- Назначение: основное write-хранилище для Auth и BoardWrite
- Порт на хосте: `3306`
- Персистентность: volume `mysql_data:/var/lib/mysql`
- Используется сервисами:
    - `auth-api` (база пользователей, пароли, доменные события)
    - `boardwrite-api` (доски, колонки, карточки, связи с метками и исполнителями)
    - `boardread-api` как fallback источник для read-модели и пользователей

### 2. cassandra

- Образ: `cassandra:4.1`
- Назначение: хранение снапшотов досок для read-модели
- Порт на хосте: `9042`
- Персистентность: volume `cassandra_data1:/var/lib/cassandra`
- При старте `boardread-api` и `boardwrite-api` создают keyspace `tasker_read` и таблицу `board_snapshots`

### 3. redpanda

- Образ: `redpandadata/redpanda:v24.2.7`
- Назначение: Kafka-совместимый брокер для доменных событий
- Порт на хосте: `19092` (внешний kafka endpoint)
- Персистентность: volume `redpanda_data:/var/lib/redpanda/data`
- Используется:
    - `auth-api` и `boardwrite-api` через общую библиотеку `Tasker.Shared.Kafka`

### 4. redis

- Образ: `redis`
- Назначение:
    - хранение активных сессий и refresh токенов Auth
    - проверка access токенов в BoardRead и BoardWrite
- Порт на хосте: `6379`
- К нему подключаются:
    - `auth-api` - `RedisAuthSessionStore`
    - `boardread-api` и `boardwrite-api` - `RedisAccessTokenValidator`

### 5. auth-api

- Dockerfile: `src/services/Auth/Tasker.Auth.Api/Dockerfile`
- Объявленный url внутри контейнера: `http://+:8080`
- Публикуется только во внутренней сети docker, наружу выходит через nginx
- Отвечает за:
    - регистрацию пользователей
    - логин и выдачу access токенов
    - хранение сессий в Redis
- Использует:
    - MySQL (`AuthDbContext`)
    - Redis (`RedisAuthSessionStore`)
    - Redpanda (Kafka продюсер для событий)

Основные HTTP эндпоинты:

- `POST /api/v1/auth/register`
    - тело: `RegisterUserCommand` (Email, DisplayName, Password)
    - результат: `RegisterUserResult` с `UserId`

- `POST /api/v1/auth/login`
    - тело: `LoginUserCommandBody { Email, Password }`
    - TTL сессии берётся из `Auth:SessionTtlMinutes`
    - результат: `LoginUserResult` с `userId`, `accessToken`, `expiresInSeconds`

### 6. boardread-api

- Dockerfile: `src/services/BoardRead/Tasker.BoardRead.Api/Dockerfile`
- Назначение: read-модель досок и пользователей
- Внутренний порт: `8080` (за nginx снаружи)
- Хранение:
    - Cassandra как основной источник снапшотов досок
    - MySQL (AuthDbContext, BoardWriteDbContext) как fallback для чтения
- Аутентификация:
    - кастомный `AccessTokenAuthenticationHandler`
    - проверка токена через Redis (`RedisAccessTokenValidator`)

Основные HTTP эндпоинты (все под `[Authorize]`):

- `GET /api/v1/boards/my`
    - возвращает список досок текущего пользователя:
    - тип ответа: `IReadOnlyCollection<BoardView>`

- `GET /api/v1/boards/{boardId}`
    - возвращает детальное представление доски:
    - тип ответа: `BoardDetailsView`
    - `404` если доска не найдена

### 7. boardwrite-api

- Dockerfile: `src/services/BoardWrite/Tasker.BoardWrite.Api/Dockerfile`
- Назначение: write-модель досок и их изменений
- Внутренний порт: `8080` (за nginx)
- Отвечает за:
    - создание и обновление досок
    - управление колонками
    - управление карточками
    - метки, исполнители, дедлайны
    - запись снапшотов в Cassandra
    - публикацию интеграционных событий в Kafka

Контроллер: `Tasker.BoardWrite.Api.Controllers.Boards.BoardsController`

Основные эндпоинты:

- Доски:
    - `GET /api/v1/boards/my` - список досок текущего пользователя (временно на write стороне)
    - `POST /api/v1/boards` - создание доски
    - `GET /api/v1/boards/{boardId}` - детали доски (временно на write стороне)
    - `GET /api/v1/boards/templates` - список доступных шаблонов досок

- Колонки:
    - `POST /api/v1/boards/{boardId}/columns` - создание новой колонки

- Карточки:
    - `POST /api/v1/boards/{boardId}/cards` - создание карточки
    - `PUT /api/v1/boards/{boardId}/cards/{cardId}` - обновление карточки
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/move` - перенос в другую колонку
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/due-date` - установка или сброс дедлайна

- Участники:
    - `POST /api/v1/boards/{boardId}/members` - добавление участника доски
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees` - назначить исполнителя
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees/remove` - снять исполнителя

- Метки:
    - `POST /api/v1/boards/{boardId}/labels` - создать метку на доске
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/labels` - назначить метку карточке
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/labels/remove` - снять метку с карточки

### 8. prometheus

- Образ: `prom/prometheus:latest`
- Назначение: сбор метрик из сервисов
- Порт на хосте: `9090`
- Конфигурация: `deploy/compose/prometheus/prometheus.yml` монтируется в `/etc/prometheus/prometheus.yml`
- Персистентность: volume `prometheus_data:/prometheus`

Сервисы отдают метрики через OpenTelemetry и Prometheus endpoint.

### 9. grafana

- Образ: `grafana/grafana:latest`
- Назначение: визуализация метрик
- Порт на хосте: `3000`
- Персистентность: volume `grafana_data:/var/lib/grafana`
- Админ логин и пароль задаются через env:
    - `GRAFANA_ADMIN_USER`
    - `GRAFANA_ADMIN_PASSWORD`
- По умолчанию авторегистрация пользователей выключена

### 10. frontend

- Каталог: `Web/tasker-ui-web`
- Стек:
    - Vite
    - React
    - TypeScript
- Контейнер таргетирует Vite dev server
- Порт на хосте: `5173`
- Основные маршруты SPA:
    - `/login` - форма входа
    - `/register` - регистрация
    - `/` - список досок текущего пользователя
    - `/boards/:boardId` - просмотр и управление конкретной доской

Авторизация на фронтенде:

- `AuthContext` хранит:
    - `userId`
    - `accessToken`
    - флаги `isAuthenticated` и `isInitialized`
- данные подтягиваются из `localStorage` на старте
- `RequireAuth` защищает приватные маршруты и редиректит на `/login` при отсутствии токена

Конфигурация API url через переменные окружения:

- `VITE_AUTH_API_BASE_URL` - база для Auth
- `VITE_BOARDREAD_API_BASE_URL` - база для BoardRead
- `VITE_BOARDWRITE_API_BASE_URL` - база для BoardWrite

В docker окружении фронтенд по умолчанию настроен на работу через nginx по адресу `http://localhost:8080`.

### 11. nginx

- Образ: `nginx:1.27-alpine`
- Публикует порт `8080` на хосте
- Конфиг: `deploy/compose/nginx/nginx.conf` монтируется в `/etc/nginx/nginx.conf`
- Работает как reverse proxy:
    - `/` - проксирование на Vite dev server `frontend:5173`
    - `/api/auth` - прокси на `auth-api`
    - `/api/boardread` - прокси на `boardread-api`
    - `/api/boardwrite` - прокси на `boardwrite-api`

Итог: при запуске через docker весь внешний трафик к API идёт через `http://localhost:8080/...`.

## Запуск

### Вариант 1 - полный стек в Docker

Предварительные шаги:

1. Скопировать файл переменных окружения:
    - из `deploy/compose/.env-example` в `deploy/compose/.env`
    - при необходимости поправить пароли и логины

2. Собрать и поднять стек:

```bash
cd deploy/compose
docker compose -f docker-compose.yml up --build -d
````

После успешного запуска:

* фронтенд через nginx: `http://localhost:8080`
* фронтенд напрямую (Vite): `http://localhost:5173`
* Prometheus: `http://localhost:9090`
* Grafana: `http://localhost:3000`

Фронтенд в docker конфигурирован ходить на API через nginx (`http://localhost:8080/api/...`).

### Вариант 2 - локальная разработка без nginx

Этот вариант удобен когда:

* сервисы .NET запускаются через `dotnet run` из IDE
* фронтенд запускается командой `npm run dev`

Типичная схема:

1. Поднять инфраструктуру (MySQL, Cassandra, Redis, Redpanda, Prometheus, Grafana) через docker или отдельные контейнеры.
2. Запустить `Auth`, `BoardWrite`, `BoardRead` локально на портах `5001`, `5003`, `5002`.
3. Для фронтенда использовать `.env.development` со значениями:

    * `VITE_AUTH_API_BASE_URL=http://localhost:5001/api/v1`
    * `VITE_BOARDREAD_API_BASE_URL=http://localhost:5002/api/v1`
    * `VITE_BOARDWRITE_API_BASE_URL=http://localhost:5003/api/v1`
4. Запустить фронтенд:

    * `cd Web/tasker-ui-web`
    * `npm install`
    * `npm run dev`
5. Открыть `http://localhost:5173`.

В этом режиме CORS включён в каждом .NET сервисе для origin `http://localhost:5173`.

## Основные сценарии использования API

1. Регистрация и логин:

    * `POST /api/v1/auth/register`
    * `POST /api/v1/auth/login`

2. Работа с досками (write):

    * `GET /api/v1/boards/my`
    * `POST /api/v1/boards`
    * `GET /api/v1/boards/{boardId}`
    * `GET /api/v1/boards/templates`

3. Работа с колонками:

    * `POST /api/v1/boards/{boardId}/columns`

4. Работа с карточками:

    * `POST /api/v1/boards/{boardId}/cards`
    * `PUT /api/v1/boards/{boardId}/cards/{cardId}`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/move`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/due-date`

5. Участники и исполнители:

    * `POST /api/v1/boards/{boardId}/members`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees/remove`

6. Метки:

    * `POST /api/v1/boards/{boardId}/labels`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/labels`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/labels/remove`

### Аутентификация

* Все эндпоинты BoardRead и BoardWrite защищены `[Authorize]`.
* Клиент обязан передавать заголовок:

    * `Authorization: Bearer <accessToken>`
* Access токен выдаётся Auth сервисом и валидируется через Redis.

## Ограничения и узкие места

Несколько известных особенностей и потенциальных проблем:

* Сильная зависимость от Redis:

    * Auth хранит в Redis активные сессии и refresh токены
    * BoardRead и BoardWrite проверяют access токены через Redis
    * при падении Redis весь доступ к API становится невозможен

* Read модель и fallback:

    * BoardRead использует Cassandra как основное хранилище снапшотов досок, но всё ещё имеет fallback в MySQL контексты Auth и BoardWrite
    * это создаёт дополнительную связанность между сервисами и увеличивает нагрузку на MySQL

* Eventual consistency:

    * изменения на write стороне публикуются в Kafka и обновляют read модель
    * в моменты нагрузки возможны короткие периоды, когда свежие изменения ещё не видны на стороне чтения

* Дополнительная прослойка nginx:

    * в docker окружении весь HTTP трафик идёт через nginx
    * это даёт единый входной пункт и удобный роутинг, но добавляет ещё один слой для диагностики при проблемах с сетью

Tasker строится как pet проект с прицелом на реалистичную архитектуру: отдельные домены, read/write модели, событийная интеграция и наблюдаемость через метрики и дашборды.
