import axios from "axios";

const backendUrl = process.env.VUE_APP_BACKEND_URL;

export function getPurchaseOrder(purchaseOrderId) {
  return axios
    .get(`${backendUrl}/PurchaseOrder/${encodeURIComponent(purchaseOrderId)}`)
    .then((response) => response.data);
}

export function getRoute(routeId) {
  return axios
    .get(`${backendUrl}/Route/${encodeURIComponent(routeId)}`)
    .then((response) => response.data);
}
