<template>
  <div class="myProfileView">
    <div class="headerSection">
      <h1 class="pageTitle">Mi Usuario</h1>

      <p class="pageSubtitle">
        Gestiona tu información personal y contraseña
      </p>
    </div>

    <div class="profileContainer">
      <div class="contentCard profileCard">
        <div class="profileHeader">
          <div class="profileAvatar">
            <i class="bi bi-person-fill avatarIcon"></i>
          </div>

          <div class="profileHeaderInfo">
            <h2>
              {{ userData.firstName }}
              {{ userData.firstLastName }}
            </h2>

            <p>{{ userData.userType }}</p>
          </div>
        </div>
        <div class="profileContent">
          <div v-if="loadError" class="errorMessage">
            {{ loadError }}
          </div>

          <div class="infoGrid">
            <div class="fieldGroup">
              <label>ID de Usuario</label>

              <div class="readonlyField">
                {{ userData.id }}
              </div>
            </div>

            <div class="fieldGroup">
              <label>Número de Identificación</label>

              <div class="readonlyField">
                {{ userData.identificationNumber }}
              </div>
            </div>
          </div>

          <div class="fieldGroup">
            <label>Correo Electrónico</label>

            <div class="readonlyField">
              {{ userData.email }}
            </div>
          </div>

          <div class="fieldGroup">
            <label>Tipo de Usuario</label>

            <div class="readonlyField">
              {{ userData.userType }}
            </div>
          </div>

          <div class="sectionDivider">
            <h3>Información Personal</h3>

            <div class="editableField">
              <label>Primer Nombre</label>

              <div class="editableRow">
                <template v-if="isEditingName">
                  <input
                    v-model="editableData.firstName"
                    type="text"
                    class="formInput"
                  />

                  <button
                    class="primaryButton"
                    :disabled="isSaving"
                    @click="saveField('name')"
                  >
                    {{ isSaving ? 'Guardando...' : 'Guardar' }}
                  </button>

                  <button
                    class="secondaryButton"
                    :disabled="isSaving"
                    @click="cancelEdit('name')"
                  >
                    Cancelar
                  </button>
                </template>

                <template v-else>
                  <div class="readonlyField editableBox">
                    {{ userData.firstName }}
                  </div>

                  <button
                    class="iconButton"
                    @click="startEdit('name')"
                  >
                    <i class="bi bi-pencil-square"></i>
                  </button>
                </template>
              </div>

              <p v-if="fieldErrors.name" class="fieldError">{{ fieldErrors.name }}</p>>
            </div>

            <div class="editableField">
              <label>Primer Apellido</label>

              <div class="editableRow">
                <template v-if="isEditingLastName1">
                  <input
                    v-model="editableData.firstLastName"
                    type="text"
                    class="formInput"
                  />

                  <button
                    class="primaryButton"
                    :disabled="isSaving"
                    @click="saveField('lastName1')"
                  >
                    {{ isSaving ? 'Guardando...' : 'Guardar' }}
                  </button>

                  <button
                    class="secondaryButton"
                    :disabled="isSaving"
                    @click="cancelEdit('lastName1')"
                  >
                    Cancelar
                  </button>
                </template>

                <template v-else>
                  <div class="readonlyField editableBox">
                    {{ userData.firstLastName }}
                  </div>

                  <button
                    class="iconButton"
                    @click="startEdit('lastName1')"
                  >
                    <i class="bi bi-pencil-square"></i>
                  </button>
                </template>
              </div>
              <p v-if="fieldErrors.lastName1" class="fieldError">{{ fieldErrors.lastName1 }}</p>
            </div>

            <div class="editableField">
              <label>Segundo Apellido</label>

              <div class="editableRow">
                <template v-if="isEditingLastName2">
                  <input
                    v-model="editableData.secondLastName"
                    type="text"
                    class="formInput"
                  />

                  <button
                    class="primaryButton"
                    :disabled="isSaving"
                    @click="saveField('lastName2')"
                  >
                    {{ isSaving ? 'Guardando...' : 'Guardar' }}
                  </button>

                  <button
                    class="secondaryButton"
                    :disabled="isSaving"
                    @click="cancelEdit('lastName2')"
                  >
                    Cancelar
                  </button>
                </template>

                <template v-else>
                  <div class="readonlyField editableBox">
                    {{ userData.secondLastName }}
                  </div>

                  <button
                    class="iconButton"
                    @click="startEdit('lastName2')"
                  >
                    <i class="bi bi-pencil-square"></i>
                  </button>
                </template>
              </div>
              <p v-if="fieldErrors.lastName2" class="fieldError">{{ fieldErrors.lastName2 }}</p>
            </div>
          </div>

          <div class="sectionDivider">
            <h3>Seguridad</h3>

            <div class="securityCard">
              <div class="securityInfo">
                <i class="bi bi-lock-fill securityIcon"></i>

                <div>
                  <p class="securityTitle">Contraseña</p>
                </div>
              </div>

              <button class="primaryButton" @click="isChangingPassword = true">
                Cambiar Contraseña
              </button>
            </div>
            <div v-if="isChangingPassword" class = "passwordForm">
              <div class="fieldGroup">
                <label>Contraseña Actual</label>

                <input
                  v-model="passwordData.currentPassword"
                  type="password"
                  class="formInput"
                />
              </div>

              <div class="fieldGroup">
                <label>Nueva Contraseña</label>

                <input
                  v-model="passwordData.newPassword"
                  type="password"
                  class="formInput"
                />
              </div>

              <p v-if="passwordError" class="fieldError">{{ passwordError }}</p>
              <p v-if="passwordSuccess" class="successMessage">{{ passwordSuccess }}</p>

              <div class="passwordActions">
                <button 
                  class="primaryButton"
                  :disabled="isSavingPassword"
                  @click="savePassword"
                >
                  {{ isSavingPassword ? 'Guardando...' : 'Guardar Cambios' }}
                </button>

                <button
                  class="secondaryButton"
                  :disabled="isSavingPassword"
                  @click="cancelPasswordChange"
                >
                  Cancelar
                </button>
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue';
import axios from 'axios';

const userData = reactive({
  id: null,
  identificationNumber: '',
  email: '',
  firstName: '',
  firstLastName: '',
  secondLastName: '',
  userType: ''
});

const editableData = reactive({
  firstName: '',
  firstLastName: '',
  secondLastName: ''
});

const passwordData = reactive({
  currentPassword: '',
  newPassword: ''
});

const isEditingName = ref(false);
const isEditingLastName1 = ref(false);
const isEditingLastName2 = ref(false);
const isChangingPassword = ref(false);

const isSaving = ref(false);
const isSavingPassword = ref(false);

const loadError = ref('');
const fieldErrors = reactive({
  name: '',
  lastName1: '',
  lastName2: ''
});

const passwordError = ref('');
const passwordSuccess = ref('');

onMounted(async () => {
  const token = localStorage.getItem("token");
  try {
    const { data } = await axios.get(`${process.env.VUE_APP_BACKEND_URL}/user/me`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    applyUserData(data); 
  } catch (error) {
    loadError.value = 'Error al cargar los datos del usuario.';
  }
});

function applyUserData(data) {
  userData.id = data.id;
  userData.identificationNumber = data.identificationNumber;
  userData.email = data.email;
  userData.firstName = data.firstName;
  userData.firstLastName = data.lastNameOne;
  userData.secondLastName = data.lastNameTwo  ?? '';
  userData.userType = mapUserType(data.type);

  editableData.firstName = userData.firstName;
  editableData.firstLastName = userData.firstLastName;
  editableData.secondLastName = userData.secondLastName;
}

function mapUserType(type) {
  const map = { Admin: 'Administrador', Operator: 'Operador' };
  return map[type] ?? type;
}

function startEdit(field) {
  editableData.firstName = userData.firstName;
  editableData.firstLastName = userData.firstLastName;
  editableData.secondLastName = userData.secondLastName;

  if (field === 'name') isEditingName.value = true;
  if (field === 'lastName1') isEditingLastName1.value = true;
  if (field === 'lastName2') isEditingLastName2.value = true;
}

async function saveField(field) {
  fieldErrors.name = '';
  fieldErrors.lastName1 = '';
  fieldErrors.lastName2 = '';
  isSaving.value = true;

  const token = localStorage.getItem("token");
  try {
    const { data } = await axios.put(`${process.env.VUE_APP_BACKEND_URL}/user/me`, 
    {
      firstName: editableData.firstName,
      lastNameOne: editableData.firstLastName,
      lastNameTwo: editableData.secondLastName || null,
    },
    { headers: { Authorization: `Bearer ${token}` } }
    );
    applyUserData(data);
    closeEditors();
  } catch (error) {
    const errorKey = field === 'name' ? 'name' : field === 'lastName1' ? 'lastName1' : 'lastName2';
    fieldErrors[errorKey] = extractErrorMessage(error) ?? 'Error al guardar los cambios.';
  } finally {
    isSaving.value = false;
  }
}

function closeEditors() {
  isEditingName.value = false;
  isEditingLastName1.value = false;
  isEditingLastName2.value = false;
}

async function savePassword() {
  passwordError.value = '';
  passwordSuccess.value = '';
  isSavingPassword.value = true;

  const token = localStorage.getItem("token");
  try {
    await axios.put(`${process.env.VUE_APP_BACKEND_URL}/user/me/password`, {
      currentPassword: passwordData.currentPassword,
      newPassword: passwordData.newPassword
    }, {
      headers: { Authorization: `Bearer ${token}` }
    });

    passwordSuccess.value = 'Contraseña actualizada correctamente.';
    passwordData.currentPassword = '';
    passwordData.newPassword = '';
  } catch (error) {
    if (error.response?.status === 401) {
      passwordError.value = 'Contraseña actual incorrecta.';
    } else {
      passwordError.value = extractErrorMessage(error) ?? 'Error al actualizar la contraseña.';
    }
  } finally {
    isSavingPassword.value = false;
  }
}

function cancelPasswordChange() {
  isChangingPassword.value = false;
  passwordData.currentPassword = '';
  passwordData.newPassword = '';
  passwordError.value = '';
  passwordSuccess.value = '';
}

function extractErrorMessage(error) {
  return error.response?.data?.message ?? error.response?.data?.Message ?? null;
}

</script>

<style scoped>
.myProfileView {
  width: 100%;
}

.headerSection {
  margin-bottom: var(--extraLargeSpacing);
}

.pageTitle {
  font-size: var(--extraLargeFontSize);
  font-weight: var(--boldFontWeight);
  color: var(--whiteTextColor);
  margin-bottom: var(--smallSpacing);
}

.pageSubtitle {
  color: var(--whiteTextColor);
}

.profileContainer {
  max-width: 1000px;
}

.profileCard {
  overflow: hidden;
  padding: 0;
}

.profileHeader {
  background: linear-gradient(
    to right,
    var(--primaryColor),
    var(--primaryColorHover)
  );

  padding: var(--extraLargeSpacing);

  display: flex;
  align-items: center;
  gap: var(--largeSpacing);
}

.profileAvatar {
  width: 90px;
  height: 90px;

  border-radius: 50%;
  background-color: var(--whiteColor);

  display: flex;
  align-items: center;
  justify-content: center;
}

.avatarIcon {
  font-size: 2.8rem;
  color: var(--primaryColor);
}

.profileHeaderInfo h2 {
  color: var(--whiteTextColor);
  margin: 0;
}

.profileHeaderInfo p {
  color: #d6e6ff;
  margin-top: var(--extraSmallSpacing);
}

.profileContent {
  padding: var(--extraLargeSpacing);
}

.infoGrid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: var(--largeSpacing);
}

.fieldGroup {
  margin-bottom: var(--largeSpacing);
}

.fieldGroup label,
.editableField label {
  display: block;
  margin-bottom: var(--smallSpacing);

  font-weight: var(--semiboldFontWeight);
  color: var(--secondaryTextColor);
}

.readonlyField {
  background-color: var(--lightBackgroundColor);
  border: 1px solid var(--tertiaryBorderColor);
  border-radius: var(--defaultBorderRadius);

  padding: var(--inputPadding);

  color: var(--primaryTextColor);
}

.sectionDivider {
  border-top: 1px solid var(--tertiaryBorderColor);

  margin-top: var(--extraLargeSpacing);
  padding-top: var(--largeSpacing);
}

.sectionDivider h3 {
  margin-bottom: var(--largeSpacing);
  color: var(--secondaryTextColor);
}

.editableField {
  margin-bottom: var(--largeSpacing);
}

.editableRow {
  display: flex;
  align-items: center;
  gap: var(--mediumSpacing);

  flex-wrap: wrap;
}

.editableBox {
  flex: 1;
}

.iconButton {
  border: none;
  background-color: transparent;

  cursor: pointer;

  padding: 10px;
  border-radius: var(--defaultBorderRadius);

  transition: background-color 0.2s ease;
}

.iconButton:hover {
  background-color: var(--navigationBackgroundColorHover);
}

.iconButton i {
  font-size: 1.1rem;
  color: var(--linkColor);
}

.securityCard {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--mediumSpacing);

  background-color: var(--lightBackgroundColor);

  border: 1px solid var(--tertiaryBorderColor);
  border-radius: var(--defaultBorderRadius);

  padding: var(--largeSpacing);
}

.securityInfo {
  display: flex;
  align-items: center;
  gap: var(--mediumSpacing);
}

.securityIcon {
  font-size: 1.4rem;
  color: var(--mutedTextColor);
}

.securityTitle {
  font-weight: var(--semiboldFontWeight);
  color: var(--secondaryTextColor);
}

@media (max-width: 768px) {
  .profileHeader {
    flex-direction: column;
    text-align: center;
  }

  .securityCard {
    flex-direction: column;
    align-items: flex-start;
  }

  .editableRow {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>