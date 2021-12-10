using DryIoc;
using ReactiveUI;

namespace Rack.Shared.DryIoc
{
    /// <summary>
    /// Класс методов расширения для контейнера внедрения зависимостей.
    /// </summary>
    public static class ContainerExs
    {
        /// <summary>
        /// Регистрирует представление и модель представления.
        /// </summary>
        /// <typeparam name="TView">Тип представления.</typeparam>
        /// <typeparam name="TViewModel">Тип модели представления.</typeparam>
        /// <param name="container">Контейнер.</param>
        /// <param name="reuse">Определяет жизненный цикл регистрируемой зависимости.</param>
        public static IContainer RegisterForNavigation<TView, TViewModel>(
            this IContainer container, IReuse reuse)
            where TViewModel : class
            where TView : IViewFor<TViewModel>
        {
            container.Register<IViewFor<TViewModel>, TView>(reuse);
            container.Register<TViewModel>(reuse);
            return container;
        }
    }
}
