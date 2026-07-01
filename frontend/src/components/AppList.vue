<template>
  <table class="app-list">
    <thead>
      <tr>
        <th v-for="column in columns" :key="column.key">
          {{ column.label }}
        </th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item[itemKey]">
        <td v-for="column in columns" :key="column.key">
          <slot
            :name="`cell-${column.key}`"
            :item="item"
            :column="column"
            :value="cellValue(item, column)"
          >
            {{ cellValue(item, column) }}
          </slot>
        </td>
      </tr>
      <tr v-if="items.length === 0">
        <td :colspan="columns.length" class="no-results">{{ emptyMessage }}</td>
      </tr>
    </tbody>
    <tfoot v-if="footerRow">
      <tr>
        <th v-for="column in columns" :key="column.key">
          <slot
            :name="`footer-${column.key}`"
            :item="footerRow"
            :column="column"
            :value="cellValue(footerRow, column)"
          >
            {{ cellValue(footerRow, column) }}
          </slot>
        </th>
      </tr>
    </tfoot>
  </table>
</template>

<script>
export default {
  name: "AppList",
  props: {
    columns: {
      type: Array,
      required: true
    },
    items: {
      type: Array,
      required: true
    },
    itemKey: {
      type: String,
      default: "id"
    },
    emptyMessage: {
      type: String,
      default: "No hay datos disponibles."
    },
    footerRow: {
      type: Object,
      default: null
    }
  },
  methods: {
    cellValue(item, column) {
      if (typeof column.value === "function") {
        return column.value(item);
      }

      return item[column.key];
    }
  }
};
</script>

<style scoped>
.app-list {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.app-list th {
  text-align: left;
  padding: 10px 16px;
  color: #555;
  font-weight: 500;
  border-bottom: 1px solid #eee;
}

.app-list td {
  padding: 14px 16px;
  border-bottom: 1px solid #f0f0f0;
  color: #333;
}

.app-list tbody tr:last-child td {
  border-bottom: none;
}

.no-results {
  text-align: center;
  color: #999;
  padding: 30px;
}
</style>
