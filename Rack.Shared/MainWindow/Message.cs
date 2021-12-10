using System;

namespace Rack.Shared.MainWindow
{
    /// <summary>
    /// Сообщение, посылаемое пользователю.
    /// </summary>
    public struct Message
    {
        public Message(
            string text,
            MessageType messageType = MessageType.Notification,
            RepresentationType representationType = RepresentationType.SmallMessage,
            Action interactionCallback = null) : this()
        {
            Text = text;
            InteractionCallback = interactionCallback;
            MessageType = messageType;
            RepresentationType = representationType;
            CreationDateTime = DateTime.UtcNow;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Text);
        }

        public bool Equals(Message other)
        {
            return string.Equals(Text, other.Text) && CreationDateTime.Equals(other.CreationDateTime);
        }

        public override bool Equals(object obj)
        {
            return obj is Message other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Text != null ? Text.GetHashCode() : 0) * 397) ^ CreationDateTime.GetHashCode();
            }
        }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Действие, выполняемое после просмотра пользователем сообщения.
        /// </summary>
        public Action InteractionCallback { get; }

        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public MessageType MessageType { get; }

        /// <summary>
        /// Тип представления сообщения.
        /// </summary>
        public RepresentationType RepresentationType { get; }

        /// <summary>
        /// Дата и время создания сообщения в UTC.
        /// </summary>
        public DateTime CreationDateTime { get; }
    }
}