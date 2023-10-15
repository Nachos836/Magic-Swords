using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    internal sealed class TextPanelScope : LifetimeScope
    {
        [SerializeField] private TMP_Text _field;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_field);
        }

        private void OnValidate()
        {
            _field ??= GetComponentInChildren<TMP_Text>();
        }
    }
}
