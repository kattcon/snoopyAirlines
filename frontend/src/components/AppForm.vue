<template>
  <form class="app-form" @submit.prevent="$emit('submit')">
    <h2 v-if="title" class="form-title">{{ title }}</h2>
    <p v-if="subtitle" class="form-subtitle">{{ subtitle }}</p>

    <div v-for="field in fields" :key="field.name" class="form-group">
      <label v-if="field.type !== 'checkbox'" :for="field.name">{{ field.label }}</label>

      <select
        v-if="field.type === 'select'"
        :id="field.name"
        :value="modelValue[field.name]"
        :class="['form-control', { 'field-error': errors[field.name] }]"
        :disabled="field.disabled"
        @change="updateField(field.name, $event.target.value)"
      >
        <option value="">{{ field.placeholder }}</option>
        <option v-for="option in field.options" :key="option.value" :value="option.value">
          {{ option.label }}
        </option>
      </select>

      <label
        v-else-if="field.type === 'checkbox'"
        :for="field.name"
        :class="['checkbox-control', { 'field-error': errors[field.name], 'is-disabled': field.disabled }]"
      >
        <input
          :id="field.name"
          type="checkbox"
          :checked="Boolean(modelValue[field.name])"
          :disabled="field.disabled"
          @change="updateField(field.name, $event.target.checked)"
        />
        <span>{{ field.label }}</span>
      </label>

      <input
        v-else
        :id="field.name"
        :value="modelValue[field.name]"
        :type="inputType(field)"
        :maxlength="field.maxlength"
        :min="field.min"
        :max="field.max"
        :step="field.step"
        :class="['form-control', { 'field-error': errors[field.name] }]"
        :placeholder="field.placeholder"
        :disabled="field.disabled"
        @input="updateField(field.name, $event.target.value)"
      />

      <span v-if="errors[field.name]" class="error-msg">{{ errors[field.name] }}</span>
    </div>

    <div class="button-group">
      <button type="button" class="btn-cancelar" @click="$emit('cancel')">
        {{ cancelLabel }}
      </button>
      <button type="submit" class="btn-registrar">
        {{ submitLabel }}
      </button>
    </div>
  </form>
</template>

<script>
export default {
  name: "AppForm",
  props: {
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
  emits: ["update:modelValue", "submit", "cancel"],
  methods: {
    inputType(field) {
      if (field.type === "datetime") return "datetime-local";
      return field.type || "text";
    },
    updateField(fieldName, value) {
      this.$emit("update:modelValue", {
        ...this.modelValue,
        [fieldName]: value
      });
    }
  }
};
</script>

<style scoped>
.app-form {
  background-color: white;
  border-radius: 15px;
  padding: 40px;
  width: min(480px, 100%);
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
  box-sizing: border-box;
}

.form-title {
  text-align: center;
  margin-bottom: 5px;
  font-size: 22px;
}

.form-subtitle {
  text-align: center;
  color: #888;
  font-size: 13px;
  margin-bottom: 25px;
}

.form-group {
  margin-bottom: 18px;
  font-size: 14px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: bold;
  color: #2c5fa8;
}

.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 14px;
  box-sizing: border-box;
  background-color: #f5f5f5;
}

.field-error {
  border-color: #e53935 !important;
}

.error-msg {
  color: #e53935;
  font-size: 12px;
  margin-top: 4px;
  display: block;
}

.form-control:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.checkbox-control {
  display: flex;
  align-items: center;
  gap: 10px;
  min-height: 40px;
  padding: 10px;
  border: 1px solid transparent;
  border-radius: 8px;
  color: #2c5fa8;
  font-weight: bold;
  box-sizing: border-box;
  cursor: pointer;
}

.checkbox-control input {
  width: 16px;
  height: 16px;
  margin: 0;
  cursor: pointer;
}

.checkbox-control.is-disabled,
.checkbox-control input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.button-group {
  display: flex;
  gap: 15px;
  margin-top: 25px;
}

.btn-cancelar {
  flex: 1;
  padding: 10px;
  background-color: white;
  border: 1px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}

.btn-registrar {
  flex: 1;
  padding: 10px;
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}
</style>
