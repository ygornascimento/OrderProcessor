import { useEffect, useState } from "react";
import { createOrder, getOrders } from "./api";
import "./App.css";

export default function App() {
  const [orders, setOrders] = useState([]);
  const [customerName, setCustomerName] = useState("");
  const [amount, setAmount] = useState("123.45");
  const [orderDate, setOrderDate] = useState("2025-12-20T10:30:00");
  const [status, setStatus] = useState("");

  async function load() {
    setStatus("Carregando...");
    try {
      const data = await getOrders();
      setOrders(data);
      setStatus("OK");
    } catch (e) {
      setStatus(e.message);
    }
  }

  async function onSubmit(e) {
    e.preventDefault();
    setStatus("Enviando...");
    try {
      const payload = {
        customerName,
        amount: Number(amount),
        orderDate,
      };

      const result = await createOrder(payload);
      setStatus(`Criado! id=${result.id}`);

      // dá um tempinho pro worker consumir e escrever no mongo
      setTimeout(load, 700);
    } catch (e) {
      setStatus(e.message);
    }
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <div style={{ padding: 24, fontFamily: "sans-serif", maxWidth: 900 }}>
      <h1>Orders Front</h1>

      <form onSubmit={onSubmit} style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
        <input
          placeholder="Customer name"
          value={customerName}
          onChange={(e) => setCustomerName(e.target.value)}
        />
        <input
          placeholder="Amount"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
        />
        <input
          placeholder="OrderDate ISO"
          value={orderDate}
          onChange={(e) => setOrderDate(e.target.value)}
          style={{ minWidth: 260 }}
        />
        <button type="submit">POST</button>
        <button type="button" onClick={load}>GET</button>
      </form>

      <p style={{ marginTop: 12 }}><b>Status:</b> {status}</p>

      <h2>Últimas orders (Mongo read-model)</h2>
      <table border="1" cellPadding="6" style={{ borderCollapse: "collapse", width: "100%" }}>
        <thead>
          <tr>
            <th>Id</th>
            <th>Customer</th>
            <th>Amount</th>
            <th>OrderDate</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((o) => (
            <tr key={o.id}>
              <td style={{ fontFamily: "monospace" }}>{o.id}</td>
              <td>{o.customerName}</td>
              <td>{o.amount}</td>
              <td>{o.orderDate}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
