const baseUrl = import.meta.env.VITE_API_BASE_URL;

export async function getOrders() {
  const res = await fetch(`${baseUrl}/api/Orders`);
  if (!res.ok) throw new Error(`GET /api/Orders falhou: ${res.status}`);
  return res.json();
}

export async function createOrder(payload) {
  const res = await fetch(`${baseUrl}/api/Orders`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!res.ok) throw new Error(`POST /api/Orders falhou: ${res.status}`);
  return res.json(); // { id }
}