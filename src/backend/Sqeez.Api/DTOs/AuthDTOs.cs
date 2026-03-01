public record LoginDTO(string Email, string Password);

public record AuthResponseDTO(
    long Id, 
    string Username, 
    string Token, 
    string Role
);