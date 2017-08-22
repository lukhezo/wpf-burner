using Castalia.Mvvm.Services.Core;

namespace Castalia.Mvvm
{
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Retrieves a service object identified by <typeparamref name="TServiceContract"/>.
        /// </summary>
        /// <typeparam name="TServiceContract">The type identifier of the service.</typeparam>
        public TServiceContract GetService<TServiceContract>() where TServiceContract : class
        {
            return ServiceContainer.Instance.GetService<TServiceContract>();
        }
    }
}

