<template>
  <div class="intake-page">
    <div class="page-top-bar">
      <button class="btn-volver" @click="$emit('back')">{{ backLabel }}</button>
    </div>

    <AppForm
      v-model="formValue"
      :title="title"
      :subtitle="subtitle"
      :fields="fields"
      :errors="errors"
      :cancel-label="cancelLabel"
      :submit-label="submitLabel"
      @cancel="$emit('cancel')"
      @submit="$emit('submit')"
    />
  </div>
</template>

<script>
import AppForm from "./AppForm.vue";

export default {
  name: "IntakePage",
  components: {
    AppForm
  },
  computed: {
    formValue: {
      get() {
        return this.modelValue;
      },
      set(value) {
        this.$emit("update:modelValue", value);
      }
    }
  },
  props: {
    backLabel: {
      type: String,
      default: "Volver"
    },
    title: {
      type: String,
      default: ""
    },
    subtitle: {
      type: String,
      default: ""
    },
    fields: {
      type: Array,
      required: true
    },
    modelValue: {
      type: Object,
      required: true
    },
    errors: {
      type: Object,
      default: () => ({})
    },
    cancelLabel: {
      type: String,
      default: "Cancelar"
    },
    submitLabel: {
      type: String,
      default: "Guardar"
    }
  },
  emits: ["update:modelValue", "back", "cancel", "submit"]
};
</script>

<style scoped>
.intake-page {
  position: relative;
  min-height: 100%;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
  padding: 24px;
  display: flex;
  justify-content: center;
  align-items: center;
  box-sizing: border-box;
}

.page-top-bar {
  position: absolute;
  top: 24px;
  right: 30px;
}

.btn-volver {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
}

.btn-volver:hover {
  background-color: #2c3e6b;
}
</style>
