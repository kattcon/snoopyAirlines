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
      </div>

      <table class="users-table">
        <thead>
          <tr>
            <th>Nombre completo</th>
            <th>Correo electrónico</th>
            <th>Rol</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id">
            <td>{{ fullName(user) }}</td>
            <td>{{ user.email }}</td>
            <td>
              <span :class="['role-badge', user.type === 0 ? 'role-admin' : 'role-operator']">
                {{ user.type === 0 ? 'Administrador' : 'Operador' }}
              </span>
            </td>
            <td>
              <span :class="['status-badge', user.pending ? 'status-pending' : 'status-active']">
                {{ user.pending ? 'Pendiente' : 'Activo' }}
              </span>
            </td>
          </tr>
          <tr v-if="filteredUsers.length === 0">
            <td colspan="4" class="no-results">No hay usuarios disponibles.</td>
          </tr>
        </tbody>
      </table>

      <p v-if="errorMsg" class="error-acceso">{{ errorMsg }}</p>

      <div class="table-footer-bar">
        <p class="table-footer">Mostrando {{ filteredUsers.length }} de {{ users.length }} usuarios</p>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "UsersList",
  data() {
    return {
      users: [],
      searchTerm: "",
      errorMsg: ""
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
        .get("https://localhost:7080/user", {
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

.users-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.users-table th {
  text-align: left;
  padding: 10px 16px;
  color: #555;
  font-weight: 500;
  border-bottom: 1px solid #eee;
}

.users-table td {
  padding: 14px 16px;
  border-bottom: 1px solid #f0f0f0;
  color: #333;
}

.users-table tbody tr:last-child td {
  border-bottom: none;
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

.no-results {
  text-align: center;
  color: #999;
  padding: 30px;
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
</style>
