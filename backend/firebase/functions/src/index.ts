import * as admin from "firebase-admin";
import { onRequest } from "firebase-functions/v2/https";

admin.initializeApp();
const db = admin.firestore();

type RequestUser = {
  uid: string;
};

type ListDoc = {
  userId: string;
  title: string;
  summary: string;
  createdAt: string;
  updatedAt: string;
};

type ListItemDoc = {
  userId: string;
  listId: string;
  text: string;
  note: string;
  position: number;
  createdAt: string;
  updatedAt: string;
};

function json(res: any, status: number, body: unknown): void {
  res.status(status).set("Content-Type", "application/json").send(JSON.stringify(body));
}

async function requireUser(req: any): Promise<RequestUser> {
  const auth = req.headers.authorization || "";
  const match = auth.match(/^Bearer (.+)$/i);
  if (!match) {
    throw new Error("unauthorized");
  }
  const token = match[1];
  const decoded = await admin.auth().verifyIdToken(token);
  return { uid: decoded.uid };
}

function nowIso(): string {
  return new Date().toISOString();
}

function getRouteSegments(req: any): string[] {
  const pathname =
    typeof req.path === "string" && req.path.length > 0
      ? req.path
      : new URL(req.url, `http://${req.headers.host}`).pathname;
  const segments = pathname.split("/").filter(Boolean);
  const versionIndex = segments.indexOf("v1");
  if (versionIndex < 0) {
    return [];
  }
  return segments.slice(versionIndex + 1);
}

function getBody(req: any): Record<string, unknown> {
  if (!req.body) {
    return {};
  }
  if (typeof req.body === "object") {
    return req.body as Record<string, unknown>;
  }
  if (typeof req.body === "string") {
    try {
      return JSON.parse(req.body) as Record<string, unknown>;
    } catch {
      return {};
    }
  }
  return {};
}

function parseTitle(value: unknown): string {
  const title = String(value ?? "").trim();
  if (title.length < 1) {
    throw new Error("title_required");
  }
  if (title.length > 120) {
    throw new Error("title_too_long");
  }
  return title;
}

function parseItemText(value: unknown): string {
  const text = String(value ?? "").trim();
  if (text.length < 1) {
    throw new Error("text_required");
  }
  if (text.length > 600) {
    throw new Error("text_too_long");
  }
  return text;
}

function parseOptionalNote(value: unknown): string {
  const note = String(value ?? "").trim();
  if (note.length > 2000) {
    throw new Error("note_too_long");
  }
  return note;
}

function parseOptionalPosition(value: unknown): number {
  if (value === undefined || value === null || value === "") {
    return Date.now();
  }
  const position = Number(value);
  if (!Number.isFinite(position)) {
    throw new Error("position_invalid");
  }
  return position;
}

function fallbackSummary(listTitle: string, items: string[]): string {
  if (items.length === 0) {
    return `This list tracks: ${listTitle}.`;
  }
  const preview = items.slice(0, 3).join(", ");
  return `This list tracks ${listTitle}. Key entries include: ${preview}.`;
}

async function generateSummary(listTitle: string, items: string[]): Promise<string> {
  return fallbackSummary(listTitle, items);
}

export const api = onRequest({ cors: true, region: "us-central1" }, async (req, res) => {
  if (req.method === "OPTIONS") {
    res.status(204).send("");
    return;
  }

  try {
    const user = await requireUser(req);
    const segments = getRouteSegments(req);
    const body = getBody(req);

    if (segments.length === 0) {
      return json(res, 404, { error: "not_found" });
    }

    if (segments[0] !== "lists") {
      return json(res, 404, { error: "not_found" });
    }

    if (req.method === "GET" && segments.length === 1) {
      const snap = await db
        .collection("lists")
        .where("userId", "==", user.uid)
                .get();
      const lists = snap.docs
        .map((doc) => ({ id: doc.id, ...(doc.data() as ListDoc) }))
        .sort((a, b) => String(b.updatedAt || "").localeCompare(String(a.updatedAt || "")));
      return json(res, 200, { lists });
    }

    if (req.method === "POST" && segments.length === 1) {
      const title = parseTitle(body.title);
      const ts = nowIso();
      const doc = await db.collection("lists").add({
        userId: user.uid,
        title,
        summary: `This list tracks: ${title}.`,
        createdAt: ts,
        updatedAt: ts,
      } satisfies ListDoc);
      const created = await doc.get();
      return json(res, 201, { list: { id: created.id, ...created.data() } });
    }

    const listId = segments[1];
    if (!listId) {
      return json(res, 404, { error: "not_found" });
    }

    const listRef = db.collection("lists").doc(listId);
    const listDoc = await listRef.get();
    if (!listDoc.exists || listDoc.data()?.userId !== user.uid) {
      return json(res, 404, { error: "list_not_found" });
    }

    if (req.method === "PATCH" && segments.length === 2) {
      const title = parseTitle(body.title);
      await listRef.update({
        title,
        updatedAt: nowIso(),
      });
      const updated = await listRef.get();
      return json(res, 200, { list: { id: updated.id, ...updated.data() } });
    }

    if (req.method === "DELETE" && segments.length === 2) {
      const itemsSnap = await db.collection("listItems").where("listId", "==", listId).get();
      const batch = db.batch();
      itemsSnap.docs.forEach((d) => batch.delete(d.ref));
      batch.delete(listRef);
      await batch.commit();
      return json(res, 200, { ok: true });
    }

    if (segments[2] === "items" && req.method === "GET" && segments.length === 3) {
      const snap = await db
        .collection("listItems")
        .where("userId", "==", user.uid)
        .where("listId", "==", listId)
                .get();
      const items = snap.docs
        .map((doc) => ({ id: doc.id, ...(doc.data() as ListItemDoc) }))
        .sort((a, b) => Number(a.position || 0) - Number(b.position || 0));
      return json(res, 200, { items });
    }

    if (segments[2] === "items" && req.method === "POST" && segments.length === 3) {
      const text = parseItemText(body.text);
      const note = parseOptionalNote(body.note);
      const position = parseOptionalPosition(body.position);
      const ts = nowIso();
      const doc = await db.collection("listItems").add({
        userId: user.uid,
        listId,
        text,
        note,
        position,
        createdAt: ts,
        updatedAt: ts,
      } satisfies ListItemDoc);
      const created = await doc.get();
      return json(res, 201, { item: { id: created.id, ...created.data() } });
    }

    if (segments[2] === "items" && segments.length === 4) {
      const itemId = segments[3];
      const itemRef = db.collection("listItems").doc(itemId);
      const itemDoc = await itemRef.get();

      if (!itemDoc.exists || itemDoc.data()?.userId !== user.uid || itemDoc.data()?.listId !== listId) {
        return json(res, 404, { error: "item_not_found" });
      }

      if (req.method === "PATCH") {
        const patch: Record<string, unknown> = { updatedAt: nowIso() };
        if (body.text !== undefined) {
          patch.text = parseItemText(body.text);
        }
        if (body.note !== undefined) {
          patch.note = parseOptionalNote(body.note);
        }
        if (body.position !== undefined) {
          patch.position = parseOptionalPosition(body.position);
        }
        if (Object.keys(patch).length === 1) {
          return json(res, 400, { error: "no_patch_fields" });
        }
        await itemRef.update(patch);
        const updated = await itemRef.get();
        return json(res, 200, { item: { id: updated.id, ...updated.data() } });
      }

      if (req.method === "DELETE") {
        await itemRef.delete();
        return json(res, 200, { ok: true });
      }
    }

    if (req.method === "POST" && segments.length === 3 && segments[2] === "regenerate-summary") {
      const itemsSnap = await db
        .collection("listItems")
        .where("userId", "==", user.uid)
        .where("listId", "==", listId)
                .get();
      const itemTexts = itemsSnap.docs
        .map((d) => d.data() as ListItemDoc)
        .sort((a, b) => Number(a.position || 0) - Number(b.position || 0))
        .map((d) => String(d.text || ""));
      const list = listDoc.data() as { title?: string };
      const summary = await generateSummary(String(list.title || "Untitled"), itemTexts);
      await listRef.update({
        summary,
        updatedAt: nowIso(),
      });
      const updated = await listRef.get();
      return json(res, 200, { list: { id: updated.id, ...updated.data() } });
    }

    return json(res, 404, { error: "not_found" });
  } catch (error: unknown) {
    const code = String((error as Error)?.message || "");
    if (code === "unauthorized") {
      return json(res, 401, { error: "unauthorized" });
    }
    if (
      code === "title_required" ||
      code === "title_too_long" ||
      code === "text_required" ||
      code === "text_too_long" ||
      code === "note_too_long" ||
      code === "position_invalid" ||
      code === "no_patch_fields"
    ) {
      return json(res, 400, { error: code });
    }
    return json(res, 500, { error: "internal_error", details: String((error as Error)?.message || error) });
  }
});




