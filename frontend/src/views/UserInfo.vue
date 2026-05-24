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
            <h3>Información Editable</h3>

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
                    @click="saveField('name')"
                  >
                    Guardar
                  </button>

                  <button
                    class="secondaryButton"
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
                    @click="isEditingName = true"
                  >
                    <i class="bi bi-pencil-square"></i>
                  </button>
                </template>
              </div>
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
                    @click="saveField('lastName1')"
                  >
                    Guardar
                  </button>

                  <button
                    class="secondaryButton"
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
                    @click="isEditingLastName1 = true"
                  >
                    <i class="bi bi-pencil-square"></i>
                  </button>
                </template>
              </div>
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
                    @click="saveField('lastName2')"
                  >
                    Guardar
                  </button>

                  <button
                    class="secondaryButton"
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
                    @click="isEditingLastName2 = true"
                  >
                    <i class="bi bi-pencil-square"></i>
                  </button>
                </template>
              </div>
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

              <button class="primaryButton">
                Cambiar Contraseña
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue';

/*
  Datos hardcodeados por ahora, pendiente de obtener desde backend
*/
const userData = reactive({
  id: 1001,
  identificationNumber: '1-2345-6789',
  email: 'admin@snoopyairlines.com',
  firstName: 'Juan',
  firstLastName: 'Pérez',
  secondLastName: 'González',
  userType: 'Admin',
});

const editableData = reactive({
  firstName: userData.firstName,
  firstLastName: userData.firstLastName,
  secondLastName: userData.secondLastName,
});

const isEditingName = ref(false);
const isEditingLastName1 = ref(false);
const isEditingLastName2 = ref(false);

function saveField(field) {
  switch (field) {
    case 'name':
      userData.firstName = editableData.firstName;
      isEditingName.value = false;
      break;

    case 'lastName1':
      userData.firstLastName = editableData.firstLastName;
      isEditingLastName1.value = false;
      break;

    case 'lastName2':
      userData.secondLastName = editableData.secondLastName;
      isEditingLastName2.value = false;
      break;
  }
}

function cancelEdit(field) {
  switch (field) {
    case 'name':
      editableData.firstName = userData.firstName;
      isEditingName.value = false;
      break;

    case 'lastName1':
      editableData.firstLastName = userData.firstLastName;
      isEditingLastName1.value = false;
      break;

    case 'lastName2':
      editableData.secondLastName = userData.secondLastName;
      isEditingLastName2.value = false;
      break;
  }
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