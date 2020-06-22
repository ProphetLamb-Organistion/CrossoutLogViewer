using System;

namespace CrossoutLogView.GUI.Core
{
    public abstract class CollectionViewModelBase : ViewModelBase, ICollectionViewModel
    {
        protected abstract void UpdateCollections();

        public void UpdateCollectionsSafe()
        {
            try
            {
                UpdateCollections();
            }
            catch (Exception) { }
        }
    }
}
