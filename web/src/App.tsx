import { FormEvent, useEffect, useMemo, useState } from "react";
import type { User } from "firebase/auth";
import {
  createUserWithEmailAndPassword,
  onAuthStateChanged,
  signInWithEmailAndPassword,
  signOut,
} from "firebase/auth";
import { auth } from "./firebase";
import * as api from "./api";
import type { ListItemModel, ListModel } from "./types";

function AuthPanel() {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setError("");
    setBusy(true);
    try {
      if (mode === "login") {
        await signInWithEmailAndPassword(auth, email.trim(), password);
      } else {
        await createUserWithEmailAndPassword(auth, email.trim(), password);
      }
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="auth-wrap">
      <h1>Summary Lists</h1>
      <p>Sign in to manage your lists and AI summaries.</p>
      <form className="auth-form" onSubmit={submit}>
        <input
          autoComplete="email"
          placeholder="Email"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
        />
        <input
          autoComplete={mode === "login" ? "current-password" : "new-password"}
          placeholder="Password"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
        />
        <button type="submit" disabled={busy}>
          {busy ? "Working..." : mode === "login" ? "Log In" : "Create Account"}
        </button>
      </form>
      <button
        className="ghost"
        onClick={() => setMode(mode === "login" ? "register" : "login")}
        disabled={busy}
      >
        {mode === "login" ? "Need an account? Register" : "Have an account? Log in"}
      </button>
      {error ? <p className="error">{error}</p> : null}
    </div>
  );
}

function AppContent({ user }: { user: User }) {
  const [lists, setLists] = useState<ListModel[]>([]);
  const [items, setItems] = useState<ListItemModel[]>([]);
  const [selectedListId, setSelectedListId] = useState("");
  const [newListTitle, setNewListTitle] = useState("");
  const [editListTitle, setEditListTitle] = useState("");
  const [newItemText, setNewItemText] = useState("");
  const [newItemNote, setNewItemNote] = useState("");
  const [busyLists, setBusyLists] = useState(false);
  const [busyItems, setBusyItems] = useState(false);
  const [busyRegenerateSummary, setBusyRegenerateSummary] = useState(false);
  const [error, setError] = useState("");

  const selectedList = useMemo(
    () => lists.find((list) => list.id === selectedListId) ?? null,
    [lists, selectedListId],
  );

  useEffect(() => {
    if (selectedList) {
      setEditListTitle(selectedList.title);
    }
  }, [selectedList]);

  const loadLists = async () => {
    setBusyLists(true);
    setError("");
    try {
      const result = await api.fetchLists(user);
      setLists(result);
      if (result.length === 0) {
        setSelectedListId("");
      } else if (!result.some((list) => list.id === selectedListId)) {
        setSelectedListId(result[0].id);
      }
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setBusyLists(false);
    }
  };

  const loadItems = async (listId: string) => {
    setBusyItems(true);
    setError("");
    try {
      const result = await api.fetchItems(user, listId);
      setItems(result);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setBusyItems(false);
    }
  };

  useEffect(() => {
    void loadLists();
  }, []);

  useEffect(() => {
    if (!selectedListId) {
      setItems([]);
      return;
    }
    void loadItems(selectedListId);
  }, [selectedListId]);

  const addList = async (event: FormEvent) => {
    event.preventDefault();
    setError("");
    try {
      const list = await api.createList(user, newListTitle);
      setNewListTitle("");
      setLists((prev) => [list, ...prev]);
      setSelectedListId(list.id);
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const saveListTitle = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedList) {
      return;
    }
    setError("");
    try {
      const updated = await api.updateList(user, selectedList.id, editListTitle);
      setLists((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const removeList = async () => {
    if (!selectedList || !window.confirm(`Delete "${selectedList.title}"?`)) {
      return;
    }
    setError("");
    try {
      await api.deleteList(user, selectedList.id);
      const next = lists.filter((item) => item.id !== selectedList.id);
      setLists(next);
      setSelectedListId(next[0]?.id ?? "");
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const regenerateSummary = async () => {
    if (!selectedList || busyRegenerateSummary) {
      return;
    }
    setError("");
    setBusyRegenerateSummary(true);
    try {
      const updated = await api.regenerateSummary(user, selectedList.id);
      setLists((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setBusyRegenerateSummary(false);
    }
  };

  const addItem = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedList) {
      return;
    }
    setError("");
    try {
      const position = items.length === 0 ? 1 : Math.max(...items.map((item) => item.position)) + 1;
      const created = await api.createItem(user, selectedList.id, {
        text: newItemText,
        note: newItemNote,
        position,
      });
      setNewItemText("");
      setNewItemNote("");
      setItems((prev) => [...prev, created].sort((a, b) => a.position - b.position));
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const editItem = async (item: ListItemModel) => {
    if (!selectedList) {
      return;
    }
    const text = window.prompt("Edit item text", item.text);
    if (text === null) {
      return;
    }
    const note = window.prompt("Edit note", item.note ?? "");
    if (note === null) {
      return;
    }
    setError("");
    try {
      const updated = await api.updateItem(user, selectedList.id, item.id, { text, note });
      setItems((prev) => prev.map((it) => (it.id === updated.id ? updated : it)));
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const removeItem = async (item: ListItemModel) => {
    if (!selectedList || !window.confirm("Delete this item?")) {
      return;
    }
    setError("");
    try {
      await api.deleteItem(user, selectedList.id, item.id);
      setItems((prev) => prev.filter((it) => it.id !== item.id));
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <div className="app-shell">
      <header className="top-bar">
        <div>
          <h1>Summary Lists</h1>
          <p>{user.email}</p>
        </div>
        <button onClick={() => signOut(auth)}>Log out</button>
      </header>

      <section className="panel">
        <form className="inline-form" onSubmit={addList}>
          <input
            placeholder="Create a new list..."
            value={newListTitle}
            onChange={(event) => setNewListTitle(event.target.value)}
            required
          />
          <button type="submit">Add List</button>
        </form>
      </section>

      <section className="panel">
        <div className="tabs">
          {lists.map((list) => (
            <button
              key={list.id}
              className={list.id === selectedListId ? "tab active" : "tab"}
              onClick={() => setSelectedListId(list.id)}
            >
              {list.title}
            </button>
          ))}
        </div>
        {busyLists ? <p>Loading lists...</p> : null}
      </section>

      {selectedList ? (
        <section className="panel">
          <div className={busyRegenerateSummary ? "summary-card summary-card-loading" : "summary-card"}>
            <div>
              <h2>{selectedList.title}</h2>
              <p>{selectedList.summary || "No summary yet."}</p>
            </div>
            <div className="summary-actions">
              <button
                className={busyRegenerateSummary ? "regen-btn loading" : "regen-btn"}
                onClick={regenerateSummary}
                disabled={busyRegenerateSummary}
              >
                {busyRegenerateSummary ? "Regenerating..." : "Regenerate Summary"}
              </button>
              {busyRegenerateSummary ? <span className="summary-status">Generating summary...</span> : null}
            </div>
          </div>

          <form className="inline-form" onSubmit={saveListTitle}>
            <input
              value={editListTitle}
              onChange={(event) => setEditListTitle(event.target.value)}
              required
            />
            <button type="submit">Rename List</button>
            <button type="button" className="danger" onClick={removeList}>
              Delete List
            </button>
          </form>

          <form className="inline-form" onSubmit={addItem}>
            <input
              placeholder="New item text..."
              value={newItemText}
              onChange={(event) => setNewItemText(event.target.value)}
              required
            />
            <input
              placeholder="Optional note..."
              value={newItemNote}
              onChange={(event) => setNewItemNote(event.target.value)}
            />
            <button type="submit">Add Item</button>
          </form>

          {busyItems ? <p>Loading items...</p> : null}
          <div className="item-list">
            {items.map((item) => (
              <article key={item.id} className="item-row">
                <div>
                  <h3>{item.text}</h3>
                  {item.note ? <p>{item.note}</p> : null}
                </div>
                <div className="actions">
                  <button className="ghost" onClick={() => editItem(item)}>
                    Edit
                  </button>
                  <button className="danger" onClick={() => removeItem(item)}>
                    Delete
                  </button>
                </div>
              </article>
            ))}
            {items.length === 0 && !busyItems ? <p>No items yet.</p> : null}
          </div>
        </section>
      ) : (
        <section className="panel">
          <p>Create a list to get started.</p>
        </section>
      )}

      {error ? <p className="error">{error}</p> : null}
    </div>
  );
}

export default function App() {
  const [authReady, setAuthReady] = useState(false);
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    return onAuthStateChanged(auth, (next) => {
      setUser(next);
      setAuthReady(true);
    });
  }, []);

  if (!authReady) {
    return <div className="auth-wrap">Loading...</div>;
  }

  if (!user) {
    return <AuthPanel />;
  }

  return <AppContent user={user} />;
}
