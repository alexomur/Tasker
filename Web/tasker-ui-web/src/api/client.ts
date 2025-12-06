const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5003";

// Явная база для write-сервиса (BoardWrite)
const API_WRITE_BASE_URL =
  import.meta.env.VITE_BOARDWRITE_API_BASE_URL ?? API_BASE_URL;

// Явная база для read-сервиса (BoardRead).
// По умолчанию — http://localhost:5002, даже если VITE_API_BASE_URL указывает на 5003.
const API_READ_BASE_URL =
  import.meta.env.VITE_BOARDREAD_API_BASE_URL ?? "http://localhost:5002";

async function request<TResponse>(
  baseUrl: string,
  path: string,
  options: RequestInit = {}
): Promise<TResponse> {
  const token = localStorage.getItem("accessToken");

  const headers = new Headers(options.headers ?? {});

  if (!headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (token && !headers.has("Authorization")) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    if (response.status === 401 || response.status === 403) {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userId");

      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }

      throw new Error("Не авторизован. Пожалуйста, войдите снова.");
    }

    let details = "";

    try {
      details = await response.text();
    } catch {
      details = "";
    }

    throw new Error(
      `HTTP ${response.status} ${response.statusText}${
        details ? `: ${details}` : ""
      }`
    );
  }

  if (response.status === 204) {
    return undefined as unknown as TResponse;
  }

  const data = (await response.json()) as TResponse;
  return data;
}

// Клиент для write-сервиса (BoardWrite, 5003 по умолчанию)
export const httpClient = {
  get<TResponse>(path: string, signal?: AbortSignal) {
    return request<TResponse>(API_WRITE_BASE_URL, path, {
      method: "GET",
      signal,
    });
  },

  post<TBody, TResponse>(path: string, body: TBody, signal?: AbortSignal) {
    return request<TResponse>(API_WRITE_BASE_URL, path, {
      method: "POST",
      body: JSON.stringify(body),
      signal,
    });
  },

  put<TBody, TResponse>(path: string, body: TBody, signal?: AbortSignal) {
    return request<TResponse>(API_WRITE_BASE_URL, path, {
      method: "PUT",
      body: JSON.stringify(body),
      signal,
    });
  },
};

// Явный алиас — write-клиент
export const httpWriteClient = httpClient;

// Отдельный клиент для read-сервиса (BoardRead, 5002 по умолчанию)
export const httpReadClient = {
  get<TResponse>(path: string, signal?: AbortSignal) {
    return request<TResponse>(API_READ_BASE_URL, path, {
      method: "GET",
      signal,
    });
  },

  post<TBody, TResponse>(path: string, body: TBody, signal?: AbortSignal) {
    return request<TResponse>(API_READ_BASE_URL, path, {
      method: "POST",
      body: JSON.stringify(body),
      signal,
    });
  },

  put<TBody, TResponse>(path: string, body: TBody, signal?: AbortSignal) {
    return request<TResponse>(API_READ_BASE_URL, path, {
      method: "PUT",
      body: JSON.stringify(body),
      signal,
    });
  },
};
