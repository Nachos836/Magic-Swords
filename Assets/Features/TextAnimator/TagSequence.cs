using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.TextAnimator
{
    public struct TagSequence
    {
        private readonly string _letters;
        private readonly IEffect _effect;

        public TagSequence(string letters, IEffect effect)
        {
            _letters = letters;
            _effect = effect;
        }

        public async UniTask ShowSequence(CancellationToken cancellation)
        {
            if (_effect is IWordEffect wordEffect)
            {
                var words = _letters.Split();
                foreach (var word in words)
                {
                    await wordEffect.RunAsync(word,cancellation);
                    
                }
            }
            else if (_effect is ICharEffect charEffect)
            {
                foreach (var letter in _letters)
                {
                    await charEffect.RunAsync(letter,cancellation);
                    
                }
            }
            else if (_effect is IRegularEffect regularEffect)
            {
                await regularEffect.RunAsync(_letters,cancellation);
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public interface IRegularEffect
    {
        Task RunAsync(string letters, CancellationToken cancellation);
    }

    public interface ICharEffect
    {
        Task RunAsync(char letter, CancellationToken cancellation);
    }

    public interface IWordEffect
    {
        Task RunAsync(string word, CancellationToken cancellation);
    }

    public interface IEffect
    {
    }
}