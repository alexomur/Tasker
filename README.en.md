# Tasker

Tasker is a kanban style web application for managing tasks and boards. The project is built as a set of services around the domain model "board - column - card", with separated write and read models and a dedicated authentication service.

Typical usage flow:
- user registers and logs in
- creates boards (from a template or from scratch)
- adds columns and cards
- assigns assignees and labels, manages due dates
- reads board data through a read model optimized for queries

## Architecture

The project is organized as a .NET 8 and React monorepo:

- backend in C# with DDD and CQRS flavor
- separate services for Auth, BoardWrite and BoardRead
- MySQL as primary write storage
- Cassandra as snapshot storage for the read model
- Redis for sessions and access token validation
- Kafka compatible broker Redpanda for integration events
- frontend on Vite + React + TypeScript
- monitoring with Prometheus and Grafana
- Nginx as a single entry point in docker environment

## Services and containers

`deploy/compose/docker-compose.yml` defines 11 containers:

### 1. mysql

- Image: `mysql:8.0`
- Purpose: primary write storage for Auth and BoardWrite
- Host port: `3306`
- Data: volume `mysql_data:/var/lib/mysql`
- Used by:
    - `auth-api` (users, passwords, domain events)
    - `boardwrite-api` (boards, columns, cards, labels, assignees)
    - `boardread-api` as a fallback source for read model and users

### 2. cassandra

- Image: `cassandra:4.1`
- Purpose: board snapshot storage for the read model
- Host port: `9042`
- Data: volume `cassandra_data1:/var/lib/cassandra`
- On startup `boardread-api` and `boardwrite-api` create:
    - keyspace `tasker_read`
    - table `board_snapshots`

### 3. redpanda

- Image: `redpandadata/redpanda:v24.2.7`
- Purpose: Kafka compatible message broker for domain and integration events
- Host port: `19092` (external kafka endpoint)
- Data: volume `redpanda_data:/var/lib/redpanda/data`
- Used by:
    - `auth-api` and `boardwrite-api` via `Tasker.Shared.Kafka`

### 4. redis

- Image: `redis`
- Purpose:
    - store active sessions and refresh tokens for Auth
    - validate access tokens for BoardRead and BoardWrite
- Host port: `6379`
- Used by:
    - `auth-api` - `RedisAuthSessionStore`
    - `boardread-api` and `boardwrite-api` - `RedisAccessTokenValidator`

### 5. auth-api

- Dockerfile: `src/services/Auth/Tasker.Auth.Api/Dockerfile`
- Internal url: `http://+:8080`
- Exposed only inside docker network, accessed through nginx from the host
- Responsibilities:
    - user registration
    - login and access token issuance
    - session storage in Redis
- Uses:
    - MySQL (`AuthDbContext`)
    - Redis (`RedisAuthSessionStore`)
    - Redpanda (Kafka producer for events)

Main HTTP endpoints:

- `POST /api/v1/auth/register`
    - body: `RegisterUserCommand` (Email, DisplayName, Password)
    - result: `RegisterUserResult` with `UserId`

- `POST /api/v1/auth/login`
    - body: `LoginUserCommandBody { Email, Password }`
    - session TTL is read from `Auth:SessionTtlMinutes`
    - result: `LoginUserResult` with `userId`, `accessToken`, `expiresInSeconds`

### 6. boardread-api

- Dockerfile: `src/services/BoardRead/Tasker.BoardRead.Api/Dockerfile`
- Purpose: read model for boards and users
- Internal port: `8080` (behind nginx externally)
- Storage:
    - Cassandra as primary snapshot store
    - MySQL (AuthDbContext, BoardWriteDbContext) as fallback for reads
- Authentication:
    - custom `AccessTokenAuthenticationHandler`
    - token validation via Redis (`RedisAccessTokenValidator`)

Main HTTP endpoints (all `[Authorize]`):

- `GET /api/v1/boards/my`
    - returns current user boards:
    - response type: `IReadOnlyCollection<BoardView>`

- `GET /api/v1/boards/{boardId}`
    - returns detailed board view:
    - response type: `BoardDetailsView`
    - `404` if board is not found

### 7. boardwrite-api

- Dockerfile: `src/services/BoardWrite/Tasker.BoardWrite.Api/Dockerfile`
- Purpose: write side for boards and their changes
- Internal port: `8080` (behind nginx)
- Responsibilities:
    - create and update boards
    - manage columns
    - manage cards
    - labels, assignees and due dates
    - snapshot writes to Cassandra
    - publishing integration events to Kafka

Controller: `Tasker.BoardWrite.Api.Controllers.Boards.BoardsController`

Main endpoints:

- Boards:
    - `GET /api/v1/boards/my` - current user boards (temporary on write side)
    - `POST /api/v1/boards` - create board
    - `GET /api/v1/boards/{boardId}` - board details (temporary on write side)
    - `GET /api/v1/boards/templates` - board templates list

- Columns:
    - `POST /api/v1/boards/{boardId}/columns` - create column

- Cards:
    - `POST /api/v1/boards/{boardId}/cards` - create card
    - `PUT /api/v1/boards/{boardId}/cards/{cardId}` - update card
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/move` - move to another column
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/due-date` - set or clear due date

- Members:
    - `POST /api/v1/boards/{boardId}/members` - add board member
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees` - assign card member
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees/remove` - unassign card member

- Labels:
    - `POST /api/v1/boards/{boardId}/labels` - create label
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/labels` - attach label to card
    - `POST /api/v1/boards/{boardId}/cards/{cardId}/labels/remove` - detach label from card

### 8. prometheus

- Image: `prom/prometheus:latest`
- Purpose: collect metrics from services
- Host port: `9090`
- Config: `deploy/compose/prometheus/prometheus.yml` mounted to `/etc/prometheus/prometheus.yml`
- Data: volume `prometheus_data:/prometheus`

Services expose metrics via OpenTelemetry and Prometheus exporter.

### 9. grafana

- Image: `grafana/grafana:latest`
- Purpose: metrics visualization
- Host port: `3000`
- Data: volume `grafana_data:/var/lib/grafana`
- Admin credentials provided via env:
    - `GRAFANA_ADMIN_USER`
    - `GRAFANA_ADMIN_PASSWORD`
- User self registration is disabled by default

### 10. frontend

- Directory: `Web/tasker-ui-web`
- Stack:
    - Vite
    - React
    - TypeScript
- Container runs Vite dev server
- Host port: `5173`
- Main SPA routes:
    - `/login` - login form
    - `/register` - registration
    - `/` - current user boards list
    - `/boards/:boardId` - single board view and management

Frontend authentication:

- `AuthContext` keeps:
    - `userId`
    - `accessToken`
    - flags `isAuthenticated` and `isInitialized`
- values are loaded from `localStorage` on startup
- `RequireAuth` protects private routes and redirects to `/login` when no token present

API urls are configured via environment variables:

- `VITE_AUTH_API_BASE_URL` - Auth base url
- `VITE_BOARDREAD_API_BASE_URL` - BoardRead base url
- `VITE_BOARDWRITE_API_BASE_URL` - BoardWrite base url

In docker environment frontend is configured to talk to APIs through nginx at `http://localhost:8080`.

### 11. nginx

- Image: `nginx:1.27-alpine`
- Host port: `8080`
- Config: `deploy/compose/nginx/nginx.conf` mounted to `/etc/nginx/nginx.conf`
- Acts as reverse proxy:
    - `/` - proxies to Vite dev server `frontend:5173`
    - `/api/auth` - proxies to `auth-api`
    - `/api/boardread` - proxies to `boardread-api`
    - `/api/boardwrite` - proxies to `boardwrite-api`

From the host perspective all API traffic goes through `http://localhost:8080/...`.

## Running

### Option 1 - full stack in Docker

Prerequisites:

1. Copy environment file:
    - from `deploy/compose/.env-example` to `deploy/compose/.env`
    - adjust credentials if needed

2. Build and start the stack:

```bash
cd deploy/compose
docker compose -f docker-compose.yml up --build -d
````

After startup:

* frontend via nginx: `http://localhost:8080`
* frontend directly (Vite): `http://localhost:5173`
* Prometheus: `http://localhost:9090`
* Grafana: `http://localhost:3000`

Frontend inside docker is configured to call APIs via nginx (`http://localhost:8080/api/...`).

### Option 2 - local development without nginx

Useful when:

* .NET services are started via `dotnet run` from IDE
* frontend is started with `npm run dev`

Typical flow:

1. Run infrastructure (MySQL, Cassandra, Redis, Redpanda, Prometheus, Grafana) with docker or separate containers.
2. Start `Auth`, `BoardWrite`, `BoardRead` locally on ports `5001`, `5003`, `5002`.
3. Use `.env.development` in frontend with:

    * `VITE_AUTH_API_BASE_URL=http://localhost:5001/api/v1`
    * `VITE_BOARDREAD_API_BASE_URL=http://localhost:5002/api/v1`
    * `VITE_BOARDWRITE_API_BASE_URL=http://localhost:5003/api/v1`
4. Start frontend:

    * `cd Web/tasker-ui-web`
    * `npm install`
    * `npm run dev`
5. Open `http://localhost:5173`.

In this mode CORS is enabled in each .NET service for origin `http://localhost:5173`.

## Main API flows

1. Registration and login:

    * `POST /api/v1/auth/register`
    * `POST /api/v1/auth/login`

2. Boards (write side):

    * `GET /api/v1/boards/my`
    * `POST /api/v1/boards`
    * `GET /api/v1/boards/{boardId}`
    * `GET /api/v1/boards/templates`

3. Columns:

    * `POST /api/v1/boards/{boardId}/columns`

4. Cards:

    * `POST /api/v1/boards/{boardId}/cards`
    * `PUT /api/v1/boards/{boardId}/cards/{cardId}`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/move`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/due-date`

5. Members and assignees:

    * `POST /api/v1/boards/{boardId}/members`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/assignees/remove`

6. Labels:

    * `POST /api/v1/boards/{boardId}/labels`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/labels`
    * `POST /api/v1/boards/{boardId}/cards/{cardId}/labels/remove`

### Authentication

* All BoardRead and BoardWrite endpoints are protected with `[Authorize]`.
* Client must send header:

    * `Authorization: Bearer <accessToken>`
* Access token is issued by the Auth service and validated via Redis.

## Limitations and bottlenecks

Some known limitations and potential bottlenecks:

* Strong dependency on Redis:

    * Auth stores active sessions and refresh tokens in Redis
    * BoardRead and BoardWrite validate access tokens via Redis
    * when Redis is down API access is effectively impossible

* Read model fallback:

    * BoardRead uses Cassandra as primary snapshot store but still has fallback to MySQL contexts of Auth and BoardWrite
    * this increases coupling between services and adds extra load on MySQL

* Eventual consistency:

    * write side publishes events to Kafka and updates the read model
    * during load there can be short periods when fresh changes are not yet visible in reads

* Extra nginx layer:

    * in docker environment all HTTP traffic goes through nginx
    * this provides a single entry point and convenient routing but adds another layer to debug when network issues appear

Tasker is built as a pet project focused on realistic architecture: separate domains, read/write models, event driven integration and observability through metrics and dashboards.
