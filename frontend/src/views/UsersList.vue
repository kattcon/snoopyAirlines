<template>
  <div class="users-page">
    <div class="page-top-bar">
      <button class="btn-volver-users" @click="$router.go(-1)">Volver</button>
    </div>

    <div class="page-header">
      <h1 class="page-title">Gestión de Usuarios</h1>
    </div>

    <div class="users-card">
      <div class="toolbar">
        <div class="search-wrapper">
          <span class="search-icon">🔍</span>
          <input
            v-model="searchTerm"
            type="text"
            class="search-input"
            placeholder="Buscar por nombre, correo o rol..."
          />
        </div>
        <button class="btn-register" @click="$router.push('/admin/register-user')">
          + Registrar usuario
        </button>
      </div>

      <AppList
        :columns="userColumns"
        :items="filteredUsers"
        empty-message="No hay usuarios disponibles."
      >
        <template #cell-fullName="{ item }">
          {{ fullName(item) }}
        </template>

        <template #cell-type="{ item }">
          <span :class="['role-badge', item.type === 0 ? 'role-admin' : 'role-operator']">
            {{ item.type === 0 ? 'Administrador' : 'Operador' }}
          </span>
        </template>

        <template #cell-pending="{ item }">
          <span :class="['status-badge', item.pending ? 'status-pending' : 'status-active']">
            {{ item.pending ? 'Pendiente' : 'Activo' }}
          </span>
        </template>

        <template #cell-actions="{ item }">
          <div v-if="!item.pending" class="actions-group">
            <button
              class="btn-edit"
              :disabled="deletingUserId === item.id"
              @click.stop="$router.push(`/admin/edit-user/${item.id}`)"
            >
              Editar
            </button>

            <button
              class="btn-delete"
              :disabled="deletingUserId === item.id"
              @click.stop="deleteUser(item)"
            >
              {{ deletingUserId === item.id ? "Eliminando..." : "Eliminar" }}
            </button>
          </div>
        </template>
      </AppList>

      <p v-if="errorMsg" class="error-acceso">{{ errorMsg }}</p>
      <p v-if="successMsg" class="success-msg">{{ successMsg }}</p>

      <div class="table-footer-bar">
        <p class="table-footer">Mostrando {{ filteredUsers.length }} de {{ users.length }} usuarios</p>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import AppList from "../components/AppList.vue";

export default {
  name: "UsersList",
  components: {
    AppList
  },
  data() {
    return {
      users: [],
      searchTerm: "",
      errorMsg: "",
      successMsg: "",
      deletingUserId: null,
      userColumns: [
        { key: "fullName", label: "Nombre completo" },
        { key: "email", label: "Correo electrónico" },
        { key: "type", label: "Rol" },
        { key: "pending", label: "Estado" },
        { key: "actions", label: "" }
      ]
    };
  },
  computed: {
    filteredUsers() {
      const term = this.searchTerm.trim().toLowerCase();
      if (!term) return this.users;
      return this.users.filter(
        (u) =>
          this.fullName(u).toLowerCase().includes(term) ||
          u.email.toLowerCase().includes(term) ||
          (u.type === 0 ? "administrador" : "operador").includes(term)
      );
    }
  },
  mounted() {
    this.loadUsers();
  },
  methods: {
    fullName(user) {
      return [user.firstName, user.lastNameOne, user.lastNameTwo]
        .filter(Boolean)
        .join(" ");
    },
    loadUsers() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/user`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.users = response.data;
        })
        .catch((error) => {
          if (error.response && (error.response.status === 401 || error.response.status === 403)) {
            this.errorMsg = "Acceso no autorizado";
          }
          console.error("Error cargando usuarios:", error);
        });
    },
    deleteUser(user) {
      const token = localStorage.getItem("token");

      if (!token) {
        this.errorMsg = "No hay sesión activa.";
        return;
      }

      const displayName = this.fullName(user) || user.email;
      const confirmed = window.confirm(`¿Seguro que deseas eliminar al usuario ${displayName}? Esta acción no se puede deshacer.`);

      if (!confirmed) {
        return;
      }

      this.errorMsg = "";
      this.successMsg = "";
      this.deletingUserId = user.id;

      axios
        .delete(`${process.env.VUE_APP_BACKEND_URL}/user/${user.id}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          this.users = this.users.filter((u) => u.id !== user.id);
          this.successMsg = "Usuario eliminado correctamente.";
        })
        .catch((error) => {
          if (error.response) {
            const backendMessage = error.response.data?.message;

            if (error.response.status === 409 && backendMessage) {
              this.errorMsg = backendMessage;
            } else if (error.response.status === 404) {
              this.errorMsg = "El usuario ya no existe.";
            } else if (error.response.status === 401 || error.response.status === 403) {
              this.errorMsg = "Acceso no autorizado para eliminar usuarios.";
            } else {
              this.errorMsg = backendMessage || "No se pudo eliminar el usuario.";
            }
          } else {
            this.errorMsg = "Error de conexión con el servidor.";
          }

          console.error("Error eliminando usuario:", error);
        })
        .finally(() => {
          this.deletingUserId = null;
        });
    }
  }
};
</script>

<style scoped>
.users-page {
  position: relative;
  min-height: 100%;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
  padding: 40px;
  box-sizing: border-box;
}

.page-top-bar {
  position: absolute;
  top: 24px;
  right: 30px;
}

.btn-volver-users {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
}

.btn-volver-users:hover {
  background-color: #2c3e6b;
}

.page-header {
  margin-bottom: 24px;
}

.btn-register {
  padding: 10px 18px;
  background-color: white;
  border: 1px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  color: #1a1a1a;
}

.btn-register:hover {
  background-color: #f5f5f5;
}

.page-title {
  font-size: 22px;
  font-weight: bold;
  margin: 0;
  color: white;
}

.users-card {
  background-color: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.search-wrapper {
  display: flex;
  align-items: center;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 8px 12px;
  width: 320px;
  background-color: #fafafa;
}

.search-icon {
  margin-right: 8px;
  font-size: 14px;
  color: #999;
}

.search-input {
  border: none;
  outline: none;
  background: transparent;
  font-size: 14px;
  width: 100%;
  color: #333;
}

.role-badge {
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 13px;
}

.role-admin {
  background-color: #e8f0fe;
  color: #3b6fd4;
}

.role-operator {
  background-color: #fff3e0;
  color: #e65100;
}

.status-badge {
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 13px;
}

.status-active {
  background-color: #e8f5e9;
  color: #2e7d32;
}

.status-pending {
  background-color: #fce4ec;
  color: #c62828;
}

.table-footer-bar {
  margin-top: 16px;
}

.table-footer {
  font-size: 13px;
  color: #888;
  margin: 0;
}

.error-acceso {
  color: #e53935;
  font-weight: bold;
  margin-top: 12px;
  text-align: center;
}

.btn-edit {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 6px 14px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
}

.btn-edit:hover {
  background-color: #2c3e6b;
}

.actions-group {
  display: flex;
  gap: 8px;
}

.btn-delete {
  background-color: #b3261e;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 6px 14px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
}

.btn-delete:hover {
  background-color: #8f1e18;
}

.btn-edit:disabled,
.btn-delete:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.success-msg {
  color: #2e7d32;
  font-weight: 700;
  margin-top: 12px;
  text-align: center;
}
</style>
