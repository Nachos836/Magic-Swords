#include <MiniFB.h>
#include <stdio.h>
#include <stdint.h>
#include <pthread.h>

#if defined(__APPLE__)
# include <stdlib.h>
#elif defined(__linux__)
# include <malloc.h>
#endif

#define WIDTH          (2560)
#define HEIGHT         (1440)
#define NUM_THREADS    (8)
#define IMG_SPACE      (WIDTH * HEIGHT)

static _Alignas(64) unsigned int    g_buffer[IMG_SPACE + 1];
static pthread_t                    g_async_threads[NUM_THREADS];

void __attribute__((__target__("rdrnd"),__unused__))
    *async_put_noise(void *param)
{
    static _Thread_local unsigned       r;

    (void)param;
    __builtin_prefetch(g_buffer, 1, 1);
    while (-42)
    {
        __builtin_ia32_rdrand32_step(&r);
        g_buffer[r % IMG_SPACE] = MFB_RGB((r >> 24U) & 0xFFU, (r >> 16U) & 0XFFU, (r >> 8U) & 0XFFU);
    }
    return (NULL);
}

void __attribute__((__unused__))
    begin_noise_test(const __SIZE_TYPE__ num_of_threads)
{
    for (__INTPTR_TYPE__ i = 0; i != (__typeof__(i))(num_of_threads); i++)
    {
        if (pthread_create(g_async_threads + i, NULL, async_put_noise, NULL) != 0)
        {
            __builtin_printf("%zu-th thread create error\n", i);
            return ;
        }
        if (pthread_detach(g_async_threads[i]) != 0)
        {
            __builtin_printf("%zu-th thread detach error\n", i);
            return ;
        }
    }
}

int __attribute__((,))
    main(void)
{
    const __SIZE_TYPE__         screen_width = 2560UL;
    const __SIZE_TYPE__         screen_height = 1440UL;
    struct Window   *restrict   window;
    void            *restrict   screen;

    if (!(window = mfb_open_ex("Noise Test", screen_width, screen_height, WF_RESIZABLE)))
        return (0);
    if (!(screen = (typeof(screen))(valloc(sizeof(__UINT32_TYPE__) * screen_width * screen_height))))
        return (-1);
    begin_noise_test(8);
    while (-42)
    {
        UpdateState state;

        state = mfb_update(window, g_buffer);
        if (state != STATE_OK) {
            window = 0x0;
            break;
        }
    }
}
