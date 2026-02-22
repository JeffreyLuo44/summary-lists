import type { User } from "firebase/auth";
import type { ListItemModel, ListModel } from "./types";

const rawBaseUrl = String(import.meta.env.VITE_API_BASE_URL || "").trim();
const baseUrl = rawBaseUrl.endsWith("/") ? rawBaseUrl.slice(0, -1) : rawBaseUrl;

async function request<T>(user: User, path: string, init?: RequestInit): Promise<T> {
  if (!baseUrl) {
    throw new Error("missing_api_base_url");
  }

  const token = await user.getIdToken();
  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...(init?.headers || {}),
    },
  });

  if (response.status === 204) {
    return {} as T;
  }

  const body = (await response.json().catch(() => ({}))) as Record<string, unknown>;
  if (!response.ok) {
    throw new Error(String(body.error || `http_${response.status}`));
  }
  return body as T;
}

export async function fetchLists(user: User): Promise<ListModel[]> {
  const data = await request<{ lists: ListModel[] }>(user, "/lists", { method: "GET" });
  return data.lists ?? [];
}

export async function createList(user: User, title: string): Promise<ListModel> {
  const data = await request<{ list: ListModel }>(user, "/lists", {
    method: "POST",
    body: JSON.stringify({ title }),
  });
  return data.list;
}

export async function updateList(user: User, listId: string, title: string): Promise<ListModel> {
  const data = await request<{ list: ListModel }>(user, `/lists/${listId}`, {
    method: "PATCH",
    body: JSON.stringify({ title }),
  });
  return data.list;
}

export async function deleteList(user: User, listId: string): Promise<void> {
  await request(user, `/lists/${listId}`, {
    method: "DELETE",
  });
}

export async function regenerateSummary(user: User, listId: string): Promise<ListModel> {
  const data = await request<{ list: ListModel }>(user, `/lists/${listId}/regenerate-summary`, {
    method: "POST",
    body: JSON.stringify({}),
  });
  return data.list;
}

export async function fetchItems(user: User, listId: string): Promise<ListItemModel[]> {
  const data = await request<{ items: ListItemModel[] }>(user, `/lists/${listId}/items`, {
    method: "GET",
  });
  return data.items ?? [];
}

export async function createItem(
  user: User,
  listId: string,
  input: { text: string; note: string; position: number },
): Promise<ListItemModel> {
  const data = await request<{ item: ListItemModel }>(user, `/lists/${listId}/items`, {
    method: "POST",
    body: JSON.stringify(input),
  });
  return data.item;
}

export async function updateItem(
  user: User,
  listId: string,
  itemId: string,
  patch: Partial<Pick<ListItemModel, "text" | "note" | "position">>,
): Promise<ListItemModel> {
  const data = await request<{ item: ListItemModel }>(user, `/lists/${listId}/items/${itemId}`, {
    method: "PATCH",
    body: JSON.stringify(patch),
  });
  return data.item;
}

export async function deleteItem(user: User, listId: string, itemId: string): Promise<void> {
  await request(user, `/lists/${listId}/items/${itemId}`, {
    method: "DELETE",
  });
}
