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

export function confirmBooking(bookingRequest) {
  return axios
    .post(`${backendUrl}/Booking`, bookingRequest)
    .then((response) => response.data);
}

export function getBooking(bookingGuid) {
  return axios
    .get(`${backendUrl}/Booking/${encodeURIComponent(bookingGuid)}`)
    .then((response) => response.data);
}
