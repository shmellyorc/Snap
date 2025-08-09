using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snap.Content.Abstractions.Interfaces;

public interface IKeyProvider
{
	// Return a 32-byte key (AES-256). Return null if not available.
	byte[]? GetArchiveKey();
}
