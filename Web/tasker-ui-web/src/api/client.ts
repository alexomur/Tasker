const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5003";

async function request<TResponse>(
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

  const response = await fetch(`${API_BASE_URL}${path}`, {
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

export const httpClient = {
  get<TResponse>(path: string, signal?: AbortSignal) {
    return request<TResponse>(path, {
      method: "GET",
      signal,
    });
  },

  post<TBody, TResponse>(
    path: string,
    body: TBody,
    signal?: AbortSignal
  ) {
    return request<TResponse>(path, {
      method: "POST",
      body: JSON.stringify(body),
      signal,
    });
  },

  put<TBody, TResponse>(
    path: string,
    body: TBody,
    signal?: AbortSignal
  ) {
    return request<TResponse>(path, {
      method: "PUT",
      body: JSON.stringify(body),
      signal,
    });
  },
};
