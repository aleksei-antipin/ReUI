using System.Collections;

namespace Abyse.ReUI
{
    public interface IUIEffect
    {
        string[] Categories { get; }
        IEnumerator PlayBackwards();
        IEnumerator Play();
    }
}