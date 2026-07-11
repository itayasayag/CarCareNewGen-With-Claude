"""
Password hashing utilities (bcrypt).

Passwords are NEVER stored in plaintext. On registration we store a one-way
bcrypt hash; on login we hash the attempt and compare. bcrypt hashes are
self-describing (they embed the salt and cost factor), so a single string is
all we store in the DB Password column.
"""
import bcrypt

# Cost factor — higher = slower = harder to brute force. 12 is a good default.
_ROUNDS = 12


def hash_password(plain: str) -> str:
    """Return a bcrypt hash (str) for a plaintext password."""
    if plain is None:
        plain = ""
    salt = bcrypt.gensalt(rounds=_ROUNDS)
    digest = bcrypt.hashpw(plain.encode("utf-8"), salt)
    return digest.decode("utf-8")


def verify_password(plain: str, hashed: str) -> bool:
    """Check a plaintext password against a stored bcrypt hash."""
    if not plain or not hashed:
        return False
    try:
        return bcrypt.checkpw(plain.encode("utf-8"), hashed.encode("utf-8"))
    except (ValueError, TypeError):
        # Malformed/legacy (non-bcrypt) stored value → treat as no match.
        return False


def is_bcrypt_hash(value: str) -> bool:
    """True if the stored value looks like a bcrypt hash (vs legacy plaintext)."""
    return isinstance(value, str) and value.startswith(("$2a$", "$2b$", "$2y$"))
