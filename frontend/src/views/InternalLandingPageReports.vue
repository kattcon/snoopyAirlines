<template>
    <div class="reports-root">
        <div class="subsection-header" v-if="!selectedReport">
            <h3 class="subsection-header-title">Reportes</h3>
            <h6 class="subsection-header-subtitle">Selecciona el informe que deseas visualizar</h6>
        </div>

        <div class="reports-selection" v-if="!selectedReport">
            <button
                v-for="report in reportEntries"
                :key="report.key"
                class="report-card"
                type="button"
                @click="openReport(report.key)"
            >
                <div class="report-card-content">
                    <div class="report-card-icon" :class="report.iconClass" aria-hidden="true">
                        <svg viewBox="0 0 24 24" role="img">
                            <path :d="report.iconPath" />
                        </svg>
                    </div>
                    <h4 class="report-card-title">{{ report.title }}</h4>
                    <p class="report-card-description">{{ report.cardDescription }}</p>
                    <div class="report-tags">
                        <span class="report-tag" v-for="tag in report.tags" :key="tag">{{ tag }}</span>
                    </div>
                </div>
                <span class="report-card-arrow" aria-hidden="true">→</span>
            </button>
        </div>

        <div class="report-detail" v-else>
            <button class="back-link" type="button" @click="goBack">← Volver a reportes</button>
            <h3 class="report-detail-title">{{ reportMeta[selectedReport].title }}</h3>
            <p class="report-detail-description">{{ reportMeta[selectedReport].detailDescription }}</p>
            <AirlineDetailedReport v-if="selectedReport === 'detailedFlight'" />
            <div v-else class="report-detail-placeholder">
                Este reporte se implementará en la siguiente etapa.
            </div>
        </div>
    </div>
</template>

<script>
    import AirlineDetailedReport from '@/components/AirlineDetailedReport.vue';

    export default {
        components: { AirlineDetailedReport },

        data() {
            return {
                selectedReport: null,
                reportMeta: {
                    detailedFlight: {
                        title: 'Vuelo detallado',
                        cardDescription: 'Consulta información granular de cada vuelo y su comportamiento operativo.',
                        detailDescription: 'Espacio reservado para el reporte con filtros y resultados detallados por vuelo.',
                        iconClass: 'report-card-icon-flight',
                        iconPath: 'M22 16v-2l-8-5V3.5A1.5 1.5 0 0 0 12.5 2h-1A1.5 1.5 0 0 0 10 3.5V9l-8 5v2l8-2.5V19l-2 1.5V22l3.5-1 3.5 1v-1.5L14 19v-5.5L22 16z',
                        tags: ['Rutas', 'Fechas', 'Asientos'],
                    },
                    monthlyRevenue: {
                        title: 'Ingresos por mes',
                        cardDescription: 'Visualiza la evolución de ingresos mensuales para el seguimiento financiero.',
                        detailDescription: 'Espacio reservado para el reporte con métricas de ingresos consolidadas por mes.',
                        iconClass: 'report-card-icon-income',
                        iconPath: 'M4 21h16v-2H4v2zM6 17h3V9H6v8zm5 0h3V5h-3v12zm5 0h3v-6h-3v6z',
                        tags: ['Ventas', 'Tendencias', 'Comparativo'],
                    },
                },
            };
        },

        computed: {
            reportEntries() {
                return Object.entries(this.reportMeta).map(([key, value]) => ({
                    key,
                    ...value,
                }));
            },
        },

        methods: {
            openReport(reportKey) {
                this.selectedReport = reportKey;
            },

            goBack() {
                this.selectedReport = null;
            },
        },
    }
</script>

<style scoped>

.reports-root {
    min-height: calc(100vh - 120px);
    display: flex;
    flex-direction: column;
}

.subsection-header {
    margin-bottom: 30px;
    max-width: 820px;
    background: rgba(255, 255, 255, 0.94);
    border: 1px solid #d2dded;
    border-radius: 14px;
    padding: 16px 20px;
    box-shadow: 0 8px 18px rgba(32, 58, 99, 0.12);
}

.subsection-header-title {
    margin: 0;
    font-size: 1.8rem;
    color: #173255;
}

.subsection-header-subtitle {
    margin: 8px 0 0;
    font-size: 1rem;
    color: #324f72;
}

.reports-selection {
    flex: 1;
    display: grid;
    grid-template-columns: repeat(2, minmax(280px, 380px));
    justify-content: center;
    align-content: center;
    gap: 24px;
}

.report-card {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border: 1px solid #d4dce7;
    border-radius: 14px;
    background-color: #ffffff;
    padding: 24px;
    text-align: left;
    cursor: pointer;
    box-shadow: 0 8px 18px rgba(32, 58, 99, 0.08);
    transition: border-color 0.25s ease, box-shadow 0.25s ease, transform 0.25s ease;
}

.report-card:hover {
    border-color: #3a7de0;
    box-shadow: 0 14px 28px rgba(40, 84, 150, 0.18);
    transform: translateY(-2px);
}

.report-card-content {
    max-width: 260px;
}

.report-card-icon {
    width: 44px;
    height: 44px;
    border-radius: 12px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 14px;
}

.report-card-icon svg {
    width: 24px;
    height: 24px;
    fill: currentColor;
}

.report-card-icon-flight {
    background: #e7f0ff;
    color: #2a5da6;
}

.report-card-icon-income {
    background: #e8f7ef;
    color: #2e8c5e;
}

.report-card-title {
    margin: 0;
    color: #203a63;
    font-size: 1.25rem;
}

.report-card-description {
    margin: 10px 0 14px;
    color: #576b84;
    line-height: 1.4;
    font-size: 0.95rem;
}

.report-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.report-tag {
    padding: 6px 10px;
    border-radius: 999px;
    background-color: #edf4ff;
    color: #2f5ea8;
    font-size: 0.78rem;
    font-weight: 600;
}

.report-card-arrow {
    margin-left: 12px;
    font-size: 1.45rem;
    color: #3a7de0;
    transition: transform 0.25s ease;
}

.report-card:hover .report-card-arrow {
    transform: translateX(6px);
}

.report-detail {
    width: 100%;
    max-width: 1300px;
    margin: 12px auto 0;
    background-color: #ffffff;
    border: 1px solid #dae4f2;
    border-radius: 16px;
    padding: 28px;
    box-shadow: 0 10px 24px rgba(32, 58, 99, 0.09);
}

.back-link {
    border: 0;
    background: none;
    color: #2f5ea8;
    font-weight: 700;
    font-size: 0.95rem;
    cursor: pointer;
    padding: 0;
}

.back-link:hover {
    text-decoration: underline;
}

.report-detail-title {
    margin: 16px 0 8px;
    color: #203a63;
    font-size: 1.7rem;
}

.report-detail-description {
    margin: 0;
    color: #5f7592;
}

.report-detail-placeholder {
    margin-top: 20px;
    border-radius: 12px;
    border: 1px dashed #b5c6e1;
    background-color: #f4f8ff;
    color: #315b97;
    padding: 16px;
    font-weight: 600;
}

@media (max-width: 950px) {
    .reports-selection {
        grid-template-columns: minmax(280px, 460px);
        align-content: start;
    }
}


</style>