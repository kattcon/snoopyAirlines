<template>
    <div class="report-container">
        <div class="filters-card">
            <h4 class="filters-title">Filtros</h4>
            <div class="filters-grid">
                <div class="filter-group">
                    <label class="filter-label">Origen</label>
                    <input v-model="filters.origin" type="text" class="filter-input" placeholder="Ej. MAD" maxlength="3" />
                </div>
                <div class="filter-group">
                    <label class="filter-label">Destino</label>
                    <input v-model="filters.destination" type="text" class="filter-input" placeholder="Ej. SJO" maxlength="3" />
                </div>
                <div class="filter-group">
                    <label class="filter-label">Clase</label>
                    <select v-model="filters.seatClass" class="filter-input">
                        <option value="">Todas</option>
                        <option value="economy">Económica</option>
                        <option value="firstClass">Primera clase</option>
                    </select>
                </div>
                <div class="filter-group">
                    <label class="filter-label">Fecha desde</label>
                    <input v-model="filters.dateFrom" type="date" class="filter-input" />
                </div>
                <div class="filter-group">
                    <label class="filter-label">Fecha hasta</label>
                    <input v-model="filters.dateTo" type="date" class="filter-input" />
                </div>
                <div class="filter-group">
                    <label class="filter-label">Aerolínea</label>
                    <input v-model="filters.airline" type="text" class="filter-input" placeholder="Ej. Snoopy Airlines" />
                </div>
            </div>
            <div class="filters-actions">
                <button class="btn-primary" type="button" @click="fetchReport" :disabled="loading">
                    {{ loading ? 'Cargando...' : 'Generar reporte' }}
                </button>
                <button class="btn-secondary" type="button" @click="resetFilters">Limpiar</button>
                <button
                    v-if="rows.length > 0"
                    class="btn-excel"
                    type="button"
                    @click="exportToExcel"
                >
                    Exportar a Excel
                </button>
            </div>
        </div>

        <p v-if="error" class="error-message">{{ error }}</p>

        <div v-if="rows.length > 0" class="table-wrapper">
            <table class="report-table">
                <thead>
                    <tr>
                        <th>Fecha</th>
                        <th>Origen</th>
                        <th>Destino</th>
                        <th># Vuelo</th>
                        <th class="th-multiline">Pasajeros<br>primera<br>clase</th>
                        <th class="th-multiline">Pasajeros<br>clase<br>económica</th>
                        <th>Aerolínea</th>
                        <th>Venta pasajeros</th>
                        <th>Venta equipajes</th>
                        <th>Total venta</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(row, index) in rows" :key="index">
                        <td>{{ formatDate(row.fecha) }}</td>
                        <td>{{ row.origen }}</td>
                        <td>{{ row.destino }}</td>
                        <td>{{ row.flightCode ?? '—' }}</td>
                        <td class="text-center">{{ row.pasajerosPrimeraClase }}</td>
                        <td class="text-center">{{ row.pasajerosEconomia }}</td>
                        <td>{{ row.aerolinea }}</td>
                        <td class="text-right">{{ formatCurrency(row.ventaPasajeros) }}</td>
                        <td class="text-right">{{ formatCurrency(row.ventaEquipajes) }}</td>
                        <td class="text-right total-cell">{{ formatCurrency(row.totalVenta) }}</td>
                    </tr>
                </tbody>
                <tfoot>
                    <tr class="totals-row">
                        <td colspan="4"><strong>Total</strong></td>
                        <td class="text-center"><strong>{{ totals.pasajerosPrimeraClase }}</strong></td>
                        <td class="text-center"><strong>{{ totals.pasajerosEconomia }}</strong></td>
                        <td></td>
                        <td class="text-right"><strong>{{ formatCurrency(totals.ventaPasajeros) }}</strong></td>
                        <td class="text-right"><strong>{{ formatCurrency(totals.ventaEquipajes) }}</strong></td>
                        <td class="text-right total-cell"><strong>{{ formatCurrency(totals.totalVenta) }}</strong></td>
                    </tr>
                </tfoot>
            </table>
        </div>

        <div v-else-if="searched && !loading" class="empty-state">
            No se encontraron vuelos con los filtros seleccionados.
        </div>
    </div>
</template>

<script>
import { getAirlineDetailedReport } from '@/services/reportService';
import * as XLSX from 'xlsx';

export default {
    name: 'AirlineDetailedReport',

    data() {
        return {
            filters: {
                origin: '',
                destination: '',
                seatClass: '',
                dateFrom: '',
                dateTo: '',
                airline: '',
            },
            rows: [],
            loading: false,
            error: null,
            searched: false,
        };
    },

    computed: {
        totals() {
            return this.rows.reduce(
                (acc, row) => {
                    acc.pasajerosPrimeraClase += row.pasajerosPrimeraClase;
                    acc.pasajerosEconomia += row.pasajerosEconomia;
                    acc.ventaPasajeros += row.ventaPasajeros;
                    acc.ventaEquipajes += row.ventaEquipajes;
                    acc.totalVenta += row.totalVenta;
                    return acc;
                },
                { pasajerosPrimeraClase: 0, pasajerosEconomia: 0, ventaPasajeros: 0, ventaEquipajes: 0, totalVenta: 0 }
            );
        },
    },

    methods: {
        async fetchReport() {
            this.loading = true;
            this.error = null;
            this.searched = true;
            try {
                this.rows = await getAirlineDetailedReport(this.filters);
            } catch {
                this.error = 'Error al cargar el reporte. Verifique los filtros e intente de nuevo.';
                this.rows = [];
            } finally {
                this.loading = false;
            }
        },

        resetFilters() {
            this.filters = { origin: '', destination: '', seatClass: '', dateFrom: '', dateTo: '', airline: '' };
            this.rows = [];
            this.error = null;
            this.searched = false;
        },

        exportToExcel() {
            const headers = [
                'Fecha', 'Origen', 'Destino', '# Vuelo',
                'Pasajeros primera clase', 'Pasajeros clase económica', 'Aerolínea',
                'Venta pasajeros', 'Venta equipajes', 'Total venta',
            ];

            const dataRows = this.rows.map((row) => [
                this.formatDate(row.fecha),
                row.origen,
                row.destino,
                row.flightCode ?? '',
                row.pasajerosPrimeraClase,
                row.pasajerosEconomia,
                row.aerolinea,
                row.ventaPasajeros,
                row.ventaEquipajes,
                row.totalVenta,
            ]);

            const totalsRow = [
                'Total', '', '', '',
                this.totals.pasajerosPrimeraClase,
                this.totals.pasajerosEconomia,
                '',
                this.totals.ventaPasajeros,
                this.totals.ventaEquipajes,
                this.totals.totalVenta,
            ];

            const ws = XLSX.utils.aoa_to_sheet([headers, ...dataRows, totalsRow]);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Reporte');
            XLSX.writeFile(wb, `reporte-aerolinea-${new Date().toISOString().slice(0, 10)}.xlsx`);
        },

        formatDate(dateStr) {
            if (!dateStr) return '';
            const [year, month, day] = dateStr.slice(0, 10).split('-');
            return `${day}/${month}/${year}`;
        },

        formatCurrency(value) {
            return new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'USD' }).format(value ?? 0);
        },
    },
};
</script>

<style scoped>
.report-container {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.filters-card {
    background: #fff;
    border: 1px solid #dae4f2;
    border-radius: 12px;
    padding: 20px 24px;
    box-shadow: 0 4px 12px rgba(32, 58, 99, 0.07);
}

.filters-title {
    margin: 0 0 16px;
    color: #203a63;
    font-size: 1rem;
    font-weight: 600;
}

.filters-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 12px;
    margin-bottom: 16px;
}

.filter-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.filter-label {
    font-size: 0.82rem;
    font-weight: 600;
    color: #4a6080;
}

.filter-input {
    padding: 8px 10px;
    border: 1px solid #ccc;
    border-radius: 8px;
    background: #f5f5f5;
    font-size: 0.9rem;
    color: #333;
}

.filter-input:focus {
    outline: none;
    border-color: #3a7de0;
    background: #fff;
}

.filters-actions {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
}

.btn-primary {
    padding: 9px 20px;
    background: #1a2b4a;
    color: #fff;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
}

.btn-primary:hover:not(:disabled) {
    background: #2c3e6b;
}

.btn-primary:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.btn-secondary {
    padding: 9px 20px;
    background: #fff;
    color: #555;
    border: 1px solid #ccc;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
}

.btn-secondary:hover {
    background: #f0f0f0;
}

.btn-excel {
    padding: 9px 20px;
    background: #1e6b3a;
    color: #fff;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
}

.btn-excel:hover {
    background: #175230;
}

.error-message {
    color: #e53935;
    font-weight: 600;
    margin: 0;
}

.table-wrapper {
    overflow-x: auto;
    border-radius: 12px;
    border: 1px solid #dae4f2;
    box-shadow: 0 4px 12px rgba(32, 58, 99, 0.07);
}

.report-table {
    width: 100%;
    border-collapse: collapse;
    background: #fff;
    font-size: 0.88rem;
}

.report-table th {
    background: #1a2b4a;
    color: #fff;
    padding: 11px 14px;
    text-align: left;
    white-space: nowrap;
}

.report-table th.th-multiline {
    white-space: normal;
    text-align: center;
    line-height: 1.4;
}

.report-table td {
    padding: 10px 14px;
    border-bottom: 1px solid #eef2f8;
    color: #333;
}

.report-table tbody tr:hover {
    background: #f4f8ff;
}

.text-center {
    text-align: center;
}

.text-right {
    text-align: right;
}

.total-cell {
    font-weight: 700;
    color: #203a63;
}

.totals-row td {
    background: #edf4ff;
    border-top: 2px solid #b5c6e1;
    padding: 11px 14px;
}

.empty-state {
    text-align: center;
    padding: 40px;
    color: #5f7592;
    background: #f4f8ff;
    border-radius: 12px;
    border: 1px dashed #b5c6e1;
}
</style>
