using MindSync.Domain.Exceptions;
using MindSync.Domain.ValueObjects;

namespace MindSync.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public User(
        Guid id,
        string firstName,
        string lastName,
        Email email,
        string passwordHash,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("O Id do utilizador não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("O nome do utilizador não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("O sobrenome do utilizador não pode ser vazio.");

        if (email == null)
            throw new DomainException("O e-mail não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash da senha não pode ser vazio.");

        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }
}
