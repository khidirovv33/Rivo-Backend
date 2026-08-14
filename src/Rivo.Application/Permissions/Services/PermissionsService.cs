using AutoMapper;
using Rivo.Application.Permissions.Dtos;
using Rivo.Application.Permissions.Interfaces;

namespace Rivo.Application.Permissions.Services;

public class PermissionsService : IPermissionsService
{
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IMapper _mapper;

    public PermissionsService(IPermissionsRepository permissionsRepository, IMapper mapper)
    {
        _permissionsRepository = permissionsRepository;
        _mapper = mapper;
    }

    public async Task<List<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionsRepository.GetAllAsync(cancellationToken);
        return permissions.Select(p => _mapper.Map<PermissionDto>(p)).ToList();
    }
}
