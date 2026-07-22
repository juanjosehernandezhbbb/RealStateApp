using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.ViewModels.Admin;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Enums;
using RealStateApp.Persistance.Contexts;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Contadores de Propiedades usando el Enum en lugar de strings
        int disponibles = await _context.Propiedades.CountAsync(p => p.Estado == EstadoPropiedad.Disponible);
        int vendidas = await _context.Propiedades.CountAsync(p => p.Estado == EstadoPropiedad.Vendida);

        // Contadores de Usuarios por Rol y Estado
        var agentes = await _userManager.GetUsersInRoleAsync("Agente");
        int agentesActivos = agentes.Count(u => u.EstaActivo);
        int agentesInactivos = agentes.Count(u => !u.EstaActivo);

        var clientes = await _userManager.GetUsersInRoleAsync("Cliente");
        int clientesActivos = clientes.Count(u => u.EstaActivo);
        int clientesInactivos = clientes.Count(u => !u.EstaActivo);

        var desarrolladores = await _userManager.GetUsersInRoleAsync("Desarrollador");
        int devActivos = desarrolladores.Count(u => u.EstaActivo);
        int devInactivos = desarrolladores.Count(u => !u.EstaActivo);

        var vm = new AdminHomeViewModel
        {
            PropiedadesDisponibles = disponibles,
            PropiedadesVendidas = vendidas,
            AgentesActivos = agentesActivos,
            AgentesInactivos = agentesInactivos,
            ClientesActivos = clientesActivos,
            ClientesInactivos = clientesInactivos,
            DesarrolladoresActivos = devActivos,
            DesarrolladoresInactivos = devInactivos
        };

        return View(vm);
    }

    public async Task<IActionResult> Agentes()
    {
        var agentes = await _userManager.GetUsersInRoleAsync("Agente");
        var vmList = new List<AgentListViewModel>();

        foreach (var agente in agentes)
        {
            // Cambiado a AgenteId
            int cantidadPropiedades = await _context.Propiedades.CountAsync(p => p.AgenteId == agente.Id);

            vmList.Add(new AgentListViewModel
            {
                Id = agente.Id,
                Nombre = agente.Nombre,
                Apellido = agente.Apellido,
                Email = agente.Email,
                CantidadPropiedades = cantidadPropiedades,
                EstaActivo = agente.EstaActivo
            });
        }

        return View(vmList);
    }

    // POST: /Admin/CambiarEstadoAgente
    [HttpPost]
    public async Task<IActionResult> CambiarEstadoAgente(string id)
    {
        var agente = await _userManager.FindByIdAsync(id);
        if (agente == null)
        {
            TempData["Error"] = "El agente seleccionado no existe.";
            return RedirectToAction(nameof(Agentes));
        }

        agente.EstaActivo = !agente.EstaActivo;
        var result = await _userManager.UpdateAsync(agente);

        if (result.Succeeded)
        {
            TempData["Success"] = agente.EstaActivo
                ? "El agente fue activado correctamente."
                : "El agente fue inactivado correctamente.";
        }
        else
        {
            TempData["Error"] = "No fue posible actualizar el estado del agente.";
        }

        return RedirectToAction(nameof(Agentes));
    }

    // GET: /Admin/EliminarAgente/{id}
    public async Task<IActionResult> EliminarAgente(string id)
    {
        var agente = await _userManager.FindByIdAsync(id);
        if (agente == null)
        {
            TempData["Error"] = "El agente seleccionado no existe.";
            return RedirectToAction(nameof(Agentes));
        }

        var vm = new AgentListViewModel
        {
            Id = agente.Id,
            Nombre = agente.Nombre,
            Apellido = agente.Apellido,
            Email = agente.Email
        };

        return View(vm);
    }

    // POST: /Admin/EliminarAgente
    [HttpPost, ActionName("EliminarAgente")]
    public async Task<IActionResult> EliminarAgenteConfirmed(string id)
    {
        try
        {
            var agente = await _userManager.FindByIdAsync(id);
            if (agente == null)
            {
                TempData["Error"] = "El agente seleccionado no existe.";
                return RedirectToAction(nameof(Agentes));
            }

            var propiedades = await _context.Propiedades
                .Where(p => p.AgenteId == id)
                .Include(p => p.PropiedadMejoras)
                .Include(p => p.Ofertas)
                .Include(p => p.Favoritos)
                .ToListAsync();

            foreach (var propiedad in propiedades)
            {
                if (propiedad.PropiedadMejoras != null)
                    _context.PropiedadMejoras.RemoveRange(propiedad.PropiedadMejoras);

                if (propiedad.Ofertas != null)
                    _context.Ofertas.RemoveRange(propiedad.Ofertas);

                if (propiedad.Favoritos != null)
                    _context.Favoritos.RemoveRange(propiedad.Favoritos);

                var mensajes = await _context.Mensajes.Where(m => m.PropiedadId == propiedad.Id).ToListAsync();
                if (mensajes.Any())
                    _context.Mensajes.RemoveRange(mensajes);
            }

            if (propiedades.Any())
                _context.Propiedades.RemoveRange(propiedades);

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(agente);
            if (result.Succeeded)
            {
                TempData["Success"] = "El agente fue eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = "No fue posible eliminar el agente. Intente nuevamente más tarde.";
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "No fue posible eliminar el agente. Intente nuevamente más tarde.";
        }

        return RedirectToAction(nameof(Agentes));
    }

    // GET: /Admin/Administradores
    public async Task<IActionResult> Administradores()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Administrador");
        var vmList = new List<AdminListViewModel>();

        foreach (var admin in admins)
        {
            vmList.Add(new AdminListViewModel
            {
                Id = admin.Id,
                Nombre = admin.Nombre,
                Apellido = admin.Apellido,
                UserName = admin.UserName,
                Cedula = admin.Cedula,
                Email = admin.Email,
                EstaActivo = admin.EstaActivo
            });
        }

        return View(vmList);
    }

    // GET: /Admin/CrearAdministrador
    public IActionResult CrearAdministrador()
    {
        return View(new CreateAdminViewModel());
    }

    // POST: /Admin/CrearAdministrador
    [HttpPost]
    public async Task<IActionResult> CrearAdministrador(CreateAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Debe completar todos los campos requeridos.";
            return View(model);
        }

        // Validar Cédula única
        var existingCedula = await _context.Users.AnyAsync(u => u.Cedula == model.Cedula);
        if (existingCedula)
        {
            ModelState.AddModelError("Cedula", "Ya existe un usuario registrado con esta cédula.");
            TempData["Error"] = "Ya existe un usuario registrado con esta cédula.";
            return View(model);
        }

        // Validar Correo único
        var existingEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario registrado con este correo electrónico.");
            TempData["Error"] = "Ya existe un usuario registrado con este correo electrónico.";
            return View(model);
        }

        // Validar Username único
        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "Ya existe un usuario registrado con este nombre de usuario.");
            TempData["Error"] = "Ya existe un usuario registrado con este nombre de usuario.";
            return View(model);
        }

        var newAdmin = new ApplicationUser
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            Cedula = model.Cedula,
            Email = model.Email,
            UserName = model.UserName,
            Telefono = "No especificado", // <- Agrega esto para evitar el error de columna nula
            EmailConfirmed = true,
            EstaActivo = true // Activo por defecto
        };

        var result = await _userManager.CreateAsync(newAdmin, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(newAdmin, "Administrador");
            TempData["Success"] = "El administrador fue creado correctamente.";
            return RedirectToAction(nameof(Administradores));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        TempData["Error"] = "No fue posible crear el administrador.";
        return View(model);
    }

    // GET: /Admin/EditarAdministrador/{id}
    public async Task<IActionResult> EditarAdministrador(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["Error"] = "No puede editar su propio usuario desde este mantenimiento.";
            return RedirectToAction(nameof(Administradores));
        }

        var admin = await _userManager.FindByIdAsync(id);
        if (admin == null || !await _userManager.IsInRoleAsync(admin, "Administrador"))
        {
            TempData["Error"] = "El administrador seleccionado no existe.";
            return RedirectToAction(nameof(Administradores));
        }

        var vm = new UpdateAdminViewModel
        {
            Id = admin.Id,
            Nombre = admin.Nombre,
            Apellido = admin.Apellido,
            Cedula = admin.Cedula,
            Email = admin.Email,
            UserName = admin.UserName
        };

        return View(vm);
    }

    // POST: /Admin/EditarAdministrador
    [HttpPost]
    public async Task<IActionResult> EditarAdministrador(UpdateAdminViewModel model)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (model.Id == currentUserId)
        {
            TempData["Error"] = "No puede editar su propio usuario desde este mantenimiento.";
            return RedirectToAction(nameof(Administradores));
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Debe completar todos los campos requeridos.";
            return View(model);
        }

        var admin = await _userManager.FindByIdAsync(model.Id);
        if (admin == null)
        {
            TempData["Error"] = "El administrador seleccionado no existe.";
            return RedirectToAction(nameof(Administradores));
        }

        // Validar Cédula única (excluyendo el usuario actual)
        var cedulaUser = await _context.Users.FirstOrDefaultAsync(u => u.Cedula == model.Cedula);
        if (cedulaUser != null && cedulaUser.Id != model.Id)
        {
            ModelState.AddModelError("Cedula", "Ya existe un usuario registrado con esta cédula.");
            TempData["Error"] = "Ya existe un usuario registrado con esta cédula.";
            return View(model);
        }

        // Validar Correo único (excluyendo el usuario actual)
        var emailUser = await _userManager.FindByEmailAsync(model.Email);
        if (emailUser != null && emailUser.Id != model.Id)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario registrado con este correo electrónico.");
            TempData["Error"] = "Ya existe un usuario registrado con este correo electrónico.";
            return View(model);
        }

        // Validar Username único (excluyendo el usuario actual)
        var nameUser = await _userManager.FindByNameAsync(model.UserName);
        if (nameUser != null && nameUser.Id != model.Id)
        {
            ModelState.AddModelError("UserName", "Ya existe un usuario registrado con este nombre de usuario.");
            TempData["Error"] = "Ya existe un usuario registrado con este nombre de usuario.";
            return View(model);
        }

        admin.Nombre = model.Nombre;
        admin.Apellido = model.Apellido;
        admin.Cedula = model.Cedula;
        admin.Email = model.Email;
        admin.UserName = model.UserName;

        // Si se ingresó una nueva contraseña
        if (!string.IsNullOrEmpty(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
            var passResult = await _userManager.ResetPasswordAsync(admin, token, model.Password);
            if (!passResult.Succeeded)
            {
                foreach (var error in passResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                TempData["Error"] = "La contraseña y la confirmación de contraseña no coinciden o no cumplen los requisitos.";
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(admin);
        if (result.Succeeded)
        {
            TempData["Success"] = "El administrador fue actualizado correctamente.";
            return RedirectToAction(nameof(Administradores));
        }

        TempData["Error"] = "No fue posible actualizar el administrador.";
        return View(model);
    }

    // POST: /Admin/CambiarEstadoAdministrador
    [HttpPost]
    public async Task<IActionResult> CambiarEstadoAdministrador(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["Error"] = "No puede inactivar a su propio usuario.";
            return RedirectToAction(nameof(Administradores));
        }

        var admin = await _userManager.FindByIdAsync(id);
        if (admin == null || !await _userManager.IsInRoleAsync(admin, "Administrador"))
        {
            TempData["Error"] = "El administrador seleccionado no existe.";
            return RedirectToAction(nameof(Administradores));
        }

        // Si está activo, verificar que no sea el último administrador activo
        if (admin.EstaActivo)
        {
            var adminRoleUsers = await _userManager.GetUsersInRoleAsync("Administrador");
            int activeAdminsCount = adminRoleUsers.Count(u => u.EstaActivo);
            if (activeAdminsCount <= 1)
            {
                TempData["Error"] = "Debe existir al menos un administrador activo en el sistema.";
                return RedirectToAction(nameof(Administradores));
            }
        }

        admin.EstaActivo = !admin.EstaActivo;
        var result = await _userManager.UpdateAsync(admin);

        if (result.Succeeded)
        {
            TempData["Success"] = admin.EstaActivo
                ? "El administrador fue activado correctamente."
                : "El administrador fue inactivado correctamente.";
        }
        else
        {
            TempData["Error"] = "No fue posible actualizar el estado del administrador.";
        }

        return RedirectToAction(nameof(Administradores));
    }

    // GET: /Admin/Desarrolladores
    public async Task<IActionResult> Desarrolladores()
    {
        var desarrolladores = await _userManager.GetUsersInRoleAsync("Desarrollador");
        var vmList = new List<DeveloperListViewModel>();

        foreach (var dev in desarrolladores)
        {
            vmList.Add(new DeveloperListViewModel
            {
                Id = dev.Id,
                Nombre = dev.Nombre,
                Apellido = dev.Apellido,
                UserName = dev.UserName,
                Cedula = dev.Cedula,
                Email = dev.Email,
                EstaActivo = dev.EstaActivo
            });
        }

        return View(vmList);
    }

    // GET: /Admin/CrearDesarrollador
    public IActionResult CrearDesarrollador()
    {
        return View(new CreateDeveloperViewModel());
    }

    // POST: /Admin/CrearDesarrollador
    [HttpPost]
    public async Task<IActionResult> CrearDesarrollador(CreateDeveloperViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Debe completar todos los campos requeridos.";
            return View(model);
        }

        // Validar Cédula única
        var existingCedula = await _context.Users.AnyAsync(u => u.Cedula == model.Cedula);
        if (existingCedula)
        {
            ModelState.AddModelError("Cedula", "Ya existe un usuario registrado con esta cédula.");
            TempData["Error"] = "Ya existe un usuario registrado con esta cédula.";
            return View(model);
        }

        // Validar Correo único
        var existingEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario registrado con este correo electrónico.");
            TempData["Error"] = "Ya existe un usuario registrado con este correo electrónico.";
            return View(model);
        }

        // Validar Username único
        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "Ya existe un usuario registrado con este nombre de usuario.");
            TempData["Error"] = "Ya existe un usuario registrado con este nombre de usuario.";
            return View(model);
        }

        var newDev = new ApplicationUser
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            Cedula = model.Cedula,
            Email = model.Email,
            UserName = model.UserName,
            Telefono = "No especificado",
            EmailConfirmed = true,
            EstaActivo = true // Activo por defecto
        };

        var result = await _userManager.CreateAsync(newDev, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(newDev, "Desarrollador");
            TempData["Success"] = "El desarrollador fue creado correctamente.";
            return RedirectToAction(nameof(Desarrolladores));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        TempData["Error"] = "No fue posible crear el desarrollador.";
        return View(model);
    }

    // GET: /Admin/EditarDesarrollador/{id}
    public async Task<IActionResult> EditarDesarrollador(string id)
    {
        var dev = await _userManager.FindByIdAsync(id);
        if (dev == null || !await _userManager.IsInRoleAsync(dev, "Desarrollador"))
        {
            TempData["Error"] = "El desarrollador seleccionado no existe.";
            return RedirectToAction(nameof(Desarrolladores));
        }

        var vm = new UpdateDeveloperViewModel
        {
            Id = dev.Id,
            Nombre = dev.Nombre,
            Apellido = dev.Apellido,
            Cedula = dev.Cedula,
            Email = dev.Email,
            UserName = dev.UserName
        };

        return View(vm);
    }

    // POST: /Admin/EditarDesarrollador
    [HttpPost]
    public async Task<IActionResult> EditarDesarrollador(UpdateDeveloperViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Debe completar todos los campos requeridos.";
            return View(model);
        }

        var dev = await _userManager.FindByIdAsync(model.Id);
        if (dev == null || !await _userManager.IsInRoleAsync(dev, "Desarrollador"))
        {
            TempData["Error"] = "El desarrollador seleccionado no existe.";
            return RedirectToAction(nameof(Desarrolladores));
        }

        // Validar Cédula única (excluyendo el usuario actual)
        var cedulaUser = await _context.Users.FirstOrDefaultAsync(u => u.Cedula == model.Cedula);
        if (cedulaUser != null && cedulaUser.Id != model.Id)
        {
            ModelState.AddModelError("Cedula", "Ya existe un usuario registrado con esta cédula.");
            TempData["Error"] = "Ya existe un usuario registrado con esta cédula.";
            return View(model);
        }

        // Validar Correo único (excluyendo el usuario actual)
        var emailUser = await _userManager.FindByEmailAsync(model.Email);
        if (emailUser != null && emailUser.Id != model.Id)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario registrado con este correo electrónico.");
            TempData["Error"] = "Ya existe un usuario registrado con este correo electrónico.";
            return View(model);
        }

        // Validar Username único (excluyendo el usuario actual)
        var nameUser = await _userManager.FindByNameAsync(model.UserName);
        if (nameUser != null && nameUser.Id != model.Id)
        {
            ModelState.AddModelError("UserName", "Ya existe un usuario registrado con este nombre de usuario.");
            TempData["Error"] = "Ya existe un usuario registrado con este nombre de usuario.";
            return View(model);
        }

        dev.Nombre = model.Nombre;
        dev.Apellido = model.Apellido;
        dev.Cedula = model.Cedula;
        dev.Email = model.Email;
        dev.UserName = model.UserName;

        // Si se ingresó una nueva contraseña
        if (!string.IsNullOrEmpty(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(dev);
            var passResult = await _userManager.ResetPasswordAsync(dev, token, model.Password);
            if (!passResult.Succeeded)
            {
                foreach (var error in passResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                TempData["Error"] = "La contraseña y la confirmación de contraseña no coinciden o no cumplen los requisitos.";
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(dev);
        if (result.Succeeded)
        {
            TempData["Success"] = "El desarrollador fue actualizado correctamente.";
            return RedirectToAction(nameof(Desarrolladores));
        }

        TempData["Error"] = "No fue posible actualizar el desarrollador.";
        return View(model);
    }

    // POST: /Admin/CambiarEstadoDesarrollador
    [HttpPost]
    public async Task<IActionResult> CambiarEstadoDesarrollador(string id)
    {
        var dev = await _userManager.FindByIdAsync(id);
        if (dev == null || !await _userManager.IsInRoleAsync(dev, "Desarrollador"))
        {
            TempData["Error"] = "El desarrollador seleccionado no existe.";
            return RedirectToAction(nameof(Desarrolladores));
        }

        dev.EstaActivo = !dev.EstaActivo;
        var result = await _userManager.UpdateAsync(dev);

        if (result.Succeeded)
        {
            TempData["Success"] = dev.EstaActivo
                ? "El desarrollador fue activado correctamente."
                : "El desarrollador fue inactivado correctamente.";
        }
        else
        {
            TempData["Error"] = "No fue posible actualizar el estado del desarrollador.";
        }

        return RedirectToAction(nameof(Desarrolladores));
    }


    // GET: /Admin/TiposPropiedades
    public async Task<IActionResult> TiposPropiedades()
    {
        var tipos = await _context.TipoPropiedades
            .Include(pt => pt.Propiedades)
            .ToListAsync();

        var vmList = tipos.Select(pt => new PropertyTypeListViewModel
        {
            Id = pt.Id,
            Nombre = pt.Nombre,
            Descripcion = pt.Descripcion,
            CantidadPropiedades = pt.Propiedades != null ? pt.Propiedades.Count : 0
        }).ToList();

        return View(vmList);
    }

    // GET: /Admin/CrearTipoPropiedad
    public IActionResult CrearTipoPropiedad()
    {
        return View(new SavePropertyTypeViewModel());
    }

    // POST: /Admin/CrearTipoPropiedad
    [HttpPost]
    public async Task<IActionResult> CrearTipoPropiedad(SavePropertyTypeViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Descripcion))
        {
            TempData["Error"] = "Debe completar todos los campos requeridos.";
            return View(model);
        }

        model.Nombre = model.Nombre.Trim();
        model.Descripcion = model.Descripcion.Trim();

        var existe = await _context.TipoPropiedades.AnyAsync(pt => pt.Nombre.ToLower() == model.Nombre.ToLower());
        if (existe)
        {
            ModelState.AddModelError("Nombre", "Ya existe un tipo de propiedad registrado con este nombre.");
            TempData["Error"] = "Ya existe un tipo de propiedad registrado con este nombre.";
            return View(model);
        }

        var tipoPropiedad = new TipoPropiedad
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion
        };

        _context.TipoPropiedades.Add(tipoPropiedad);
        await _context.SaveChangesAsync();

        TempData["Success"] = "El tipo de propiedad fue creado correctamente.";
        return RedirectToAction(nameof(TiposPropiedades));
    }

    // GET: /Admin/EditarTipoPropiedad/{id}
    public async Task<IActionResult> EditarTipoPropiedad(int id)
    {
        var tipoPropiedad = await _context.TipoPropiedades.FindAsync(id);
        if (tipoPropiedad == null)
        {
            TempData["Error"] = "El tipo de propiedad seleccionado no existe.";
            return RedirectToAction(nameof(TiposPropiedades));
        }

        var vm = new SavePropertyTypeViewModel
        {
            Id = tipoPropiedad.Id,
            Nombre = tipoPropiedad.Nombre,
            Descripcion = tipoPropiedad.Descripcion
        };

        return View(vm);
    }

    // POST: /Admin/EditarTipoPropiedad
    [HttpPost]
    public async Task<IActionResult> EditarTipoPropiedad(SavePropertyTypeViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Descripcion))
        {
            TempData["Error"] = "Debe completar todos los campos requeridos.";
            return View(model);
        }

        model.Nombre = model.Nombre.Trim();
        model.Descripcion = model.Descripcion.Trim();

        var tipoPropiedad = await _context.TipoPropiedades.FindAsync(model.Id);
        if (tipoPropiedad == null)
        {
            TempData["Error"] = "El tipo de propiedad seleccionado no existe.";
            return RedirectToAction(nameof(TiposPropiedades));
        }

        var existe = await _context.TipoPropiedades.AnyAsync(pt => pt.Nombre.ToLower() == model.Nombre.ToLower() && pt.Id != model.Id);
        if (existe)
        {
            ModelState.AddModelError("Nombre", "Ya existe otro tipo de propiedad registrado con este nombre.");
            TempData["Error"] = "Ya existe otro tipo de propiedad registrado con este nombre.";
            return View(model);
        }

        tipoPropiedad.Nombre = model.Nombre;
        tipoPropiedad.Descripcion = model.Descripcion;

        _context.TipoPropiedades.Update(tipoPropiedad);
        await _context.SaveChangesAsync();

        TempData["Success"] = "El tipo de propiedad fue actualizado correctamente.";
        return RedirectToAction(nameof(TiposPropiedades));
    }

    // POST: /Admin/EliminarTipoPropiedad
    [HttpPost]
    public async Task<IActionResult> EliminarTipoPropiedad(int id)
    {
        try
        {
            var tipoPropiedad = await _context.TipoPropiedades
                .Include(pt => pt.Propiedades)
                    .ThenInclude(p => p.Imagenes) // Cambiado a 'Imagenes'
                .Include(pt => pt.Propiedades)
                    .ThenInclude(p => p.Favoritos)
                .FirstOrDefaultAsync(pt => pt.Id == id);

            if (tipoPropiedad == null)
            {
                TempData["Error"] = "El tipo de propiedad seleccionado no existe.";
                return RedirectToAction(nameof(TiposPropiedades));
            }

            if (tipoPropiedad.Propiedades != null && tipoPropiedad.Propiedades.Any())
            {
                foreach (var prop in tipoPropiedad.Propiedades)
                {
                    if (prop.Imagenes != null) // Cambiado a 'prop.Imagenes'
                    {
                        _context.PropiedadImagenes.RemoveRange(prop.Imagenes);
                    }

                    if (prop.Favoritos != null)
                    {
                        _context.Favoritos.RemoveRange(prop.Favoritos);
                    }
                }

                _context.Propiedades.RemoveRange(tipoPropiedad.Propiedades);
            }

            _context.TipoPropiedades.Remove(tipoPropiedad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El tipo de propiedad fue eliminado correctamente.";
        }
        catch (Exception)
        {
            TempData["Error"] = "No fue posible eliminar el tipo de propiedad. Intente nuevamente más tarde.";
        }

        return RedirectToAction(nameof(TiposPropiedades));
    }
}