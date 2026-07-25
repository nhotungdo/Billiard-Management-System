using System;
using System.Collections.Generic;
using System.Linq;

namespace BilliardManagement.Web.Services
{
    public class PasswordResetItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UsernameOrPhone { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Note { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;
    }

    public static class PasswordResetStore
    {
        private static readonly List<PasswordResetItem> _requests = new();
        private static readonly object _lock = new();

        public static void AddRequest(string usernameOrPhone, string? fullName = null, string? note = null)
        {
            lock (_lock)
            {
                _requests.Add(new PasswordResetItem
                {
                    UsernameOrPhone = usernameOrPhone,
                    FullName = fullName,
                    Note = note,
                    RequestedAt = DateTime.Now
                });
            }
        }

        public static List<PasswordResetItem> GetPendingRequests()
        {
            lock (_lock)
            {
                return _requests.Where(r => !r.IsResolved).OrderByDescending(r => r.RequestedAt).ToList();
            }
        }

        public static void MarkAsResolved(Guid id)
        {
            lock (_lock)
            {
                var item = _requests.FirstOrDefault(r => r.Id == id);
                if (item != null)
                {
                    item.IsResolved = true;
                }
            }
        }
    }
}
