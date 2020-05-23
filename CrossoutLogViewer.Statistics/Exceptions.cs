using System;
using System.Runtime.Serialization;

namespace CrossoutLogView.Statistics
{
    public class PlayerNotFoundException : ArgumentException
    {
        public PlayerNotFoundException()
        {
        }

        public PlayerNotFoundException(string message) : base(message)
        {
        }

        public PlayerNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PlayerNotFoundException(string message, string paramName) : base(message, paramName)
        {
        }

        public PlayerNotFoundException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        protected PlayerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class UserNotFoundException : ArgumentException
    {
        public UserNotFoundException()
        {
        }

        public UserNotFoundException(string message) : base(message)
        {
        }

        public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UserNotFoundException(string message, string paramName) : base(message, paramName)
        {
        }

        public UserNotFoundException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        protected UserNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class WeaponNotFoundException : ArgumentException
    {
        public WeaponNotFoundException()
        {
        }

        public WeaponNotFoundException(string message) : base(message)
        {
        }

        public WeaponNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WeaponNotFoundException(string message, string paramName) : base(message, paramName)
        {
        }

        public WeaponNotFoundException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        protected WeaponNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class MatchingLogEntryNotFoundException : ArgumentException
    {
        public MatchingLogEntryNotFoundException()
        {
        }

        public MatchingLogEntryNotFoundException(string message) : base(message)
        {
        }

        public MatchingLogEntryNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MatchingLogEntryNotFoundException(string message, string paramName) : base(message, paramName)
        {
        }

        public MatchingLogEntryNotFoundException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        protected MatchingLogEntryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
