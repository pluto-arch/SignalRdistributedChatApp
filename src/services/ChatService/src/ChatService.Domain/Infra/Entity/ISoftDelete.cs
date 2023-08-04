namespace ChatService.Domain.Infra;

public interface ISoftDelete
{
    bool Deleted { get; set; }
}