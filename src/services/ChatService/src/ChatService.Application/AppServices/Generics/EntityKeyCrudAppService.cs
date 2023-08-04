using AutoMapper;
using ChatService.Application.Models.Generics;
using ChatService.Domain.Infra;
using ChatService.Uow;

namespace ChatService.Application.AppServices.Generics
{
    public class EntityKeyCrudAppService<TEntity, TKey, TDto, TGetListRequest, TListItemDto, TUpdateRequest, TCreateRequest>
    : AlternateKeyCrudAppService<TEntity, TKey, TDto, TGetListRequest, TListItemDto, TUpdateRequest, TCreateRequest>
    where TEntity : BaseEntity<TKey>
    where TGetListRequest : PageRequest
    {

        /// <inheritdoc />
        public EntityKeyCrudAppService(IUnitOfWork uow, IMapper mapper) : base(uow, mapper)
        {
        }

        protected override async Task<TEntity> GetEntityByIdAsync(TKey id) => await _repository.GetAsync(id);

        protected override async Task DeleteByIdAsync(TKey id) => await _repository.DeleteAsync(id, true);
    }
}
