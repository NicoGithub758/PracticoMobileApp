# tupenca.uy — Mobile

## Descripción

**tupenca.uy** es una plataforma web y móvil para la administración de pencas deportivas bajo una arquitectura **multi-tenant**, permitiendo que múltiples organizaciones administren sus propias instancias de forma independiente.

Cada sitio cuenta con su propia configuración, usuarios, pencas, premios y personalización visual, manteniendo el aislamiento de la información entre organizaciones.

El sistema fue desarrollado como proyecto obligatorio de la asignatura **TSI.NET 2026**.

---

## Funcionalidades principales

- Selección de sitio con logo y personalización visual.
- Autenticación mediante credenciales internas y Google (Auth0).
- Visualización de pencas disponibles y participación.
- Ingreso y modificación de predicciones.
- Tabla de posiciones en tiempo real.
- Pago de participación con PayPal (vía WebView).
- Notificaciones push (resultados, recordatorios, ranking, generales).
- Configuración individual de notificaciones.

---

## Arquitectura

El proyecto está dividido en tres repositorios:

| Repositorio | Descripción |
|-------------|-------------|
| Backend | API REST + Panel de Administración desarrollado en ASP.NET Core MVC |
| Frontend | Aplicación Web desarrollada con React + TypeScript |
| **Mobile** | **Aplicación Android desarrollada con .NET MAUI (este repositorio)** |

---

## Tecnologías utilizadas

- .NET MAUI (Android)
- C# 12
- Firebase Cloud Messaging
- Auth0
- PayPal

---

## Requisitos

Antes de ejecutar el proyecto es necesario tener instalado:

- .NET SDK 10
- Visual Studio 2022 o superior con los workloads **Desarrollo de .NET MAUI** y **Desarrollo móvil con .NET**
- Android SDK (se instala con Visual Studio)
- Git
- **API Backend corriendo localmente** (ver repositorio Backend)

---

## Instalación

Clonar el repositorio:

```bash
git clone https://github.com/NicoGithub758/PracticoMobileApp.git
```

Entrar al proyecto:

```bash
cd PracticoMobileApp
```

### Restaurar dependencias

```bash
dotnet restore
```

---

## Configuración

### URL del Backend

En `Services/ApiService.cs`, verificar que la constante `BaseUrl` apunte a la API local:

```csharp
private const string BaseUrl = "https://10.0.2.2:7230";
```

> `10.0.2.2` es el alias que el emulador Android usa para acceder al localhost de la máquina host.

### Credenciales de servicios externos

Las credenciales de Firebase hay que copiar el archivo google-services.json de la carpeta  compartida de la entrega y pegarlo en el proyecto mobile en la ruta Platforms/Android

---

## Ejecución

1. Asegurarse de que la **API Backend esté corriendo localmente** en `https://localhost:7230`.
2. Abrir la solución `PracticoMobileApp.sln` en Visual Studio.
3. Seleccionar la configuración **Debug** y un **emulador Android** o dispositivo físico con Depuración USB habilitada.
4. Ejecutar con **F5**.

---

Tecnólogo en Informática – TSI.NET 2026
