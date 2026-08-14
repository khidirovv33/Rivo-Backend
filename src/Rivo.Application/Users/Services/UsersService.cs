using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Users.Dtos;
using Rivo.Application.Users.Interfaces;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Users.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public UsersService(
        IUsersRepository usersRepository,
        IRolesRepository rolesRepository,
        IPasswordHasherService passwordHasher,
        IApplicationDbContext dbContext,
        IMapper mapper)
    {
        _usersRepository = usersRepository;
        _rolesRepository = rolesRepository;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetTenantUserOrThrowAsync(tenantId, id, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<PaginatedList<UserDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _usersRepository.GetPagedAsync(
            tenantId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken);

        var dtos = items.Select(u => _mapper.Map<UserDto>(u)).ToList();
        return new PaginatedList<UserDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequestDto request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        if (await _usersRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Email)] = new[] { "A user with this email already exists." }
            });
        }

        await EnsureRoleBelongsToTenantAsync(tenantId, request.RoleId, cancellationToken);

        var user = new User
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            RoleId = request.RoleId,
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedBy = createdBy
        };

        await _usersRepository.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(tenantId, user.Id, cancellationToken);
    }

    public async Task<UserDto> UpdateAsync(Guid tenantId, Guid id, UpdateUserRequestDto request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        var user = await GetTenantUserOrThrowAsync(tenantId, id, cancellationToken);
        await EnsureRoleBelongsToTenantAsync(tenantId, request.RoleId, cancellationToken);

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.RoleId = request.RoleId;
        user.Status = request.Status;
        user.UpdatedBy = updatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(tenantId, user.Id, cancellationToken);
    }

    public async Task BlockAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetTenantUserOrThrowAsync(tenantId, id, cancellationToken);
        user.Status = UserStatus.Blocked;
        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnblockAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetTenantUserOrThrowAsync(tenantId, id, cancellationToken);
        user.Status = UserStatus.Active;
        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetTenantUserOrThrowAsync(tenantId, id, cancellationToken);
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> GetTenantUserOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);

        if (user.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return user;
    }

    private async Task EnsureRoleBelongsToTenantAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken)
    {
        var role = await _rolesRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), roleId);

        if (role.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }
    }
}
