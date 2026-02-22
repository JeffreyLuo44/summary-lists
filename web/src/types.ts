export type ListModel = {
  id: string;
  userId: string;
  title: string;
  summary: string;
  createdAt: string;
  updatedAt: string;
};

export type ListItemModel = {
  id: string;
  userId: string;
  listId: string;
  text: string;
  note: string;
  position: number;
  createdAt: string;
  updatedAt: string;
};
