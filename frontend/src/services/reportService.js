import axios from "axios";

const backendUrl = process.env.VUE_APP_BACKEND_URL;

export function getAirlineDetailedReport(filters) {
    const token = localStorage.getItem("token");
    const params = {};
    if (filters.origin) params.origin = filters.origin;
    if (filters.destination) params.destination = filters.destination;
    if (filters.seatClass) params.seatClass = filters.seatClass;
    if (filters.dateFrom) params.dateFrom = filters.dateFrom;
    if (filters.dateTo) params.dateTo = filters.dateTo;

    return axios
        .get(`${backendUrl}/Report/airline-detailed`, {
            params,
            headers: { Authorization: `Bearer ${token}` },
        })
        .then((response) => response.data);
}
