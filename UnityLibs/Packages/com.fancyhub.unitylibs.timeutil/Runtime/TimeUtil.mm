#include <time.h>

extern "C" {
    long long clock_gettime_ios() {
        struct timespec ts;
        clock_gettime(CLOCK_MONOTONIC_RAW, &ts);

        long long milliseconds = ts.tv_sec * 1000 + ts.tv_nsec / 1000000;        
        return milliseconds;
    }
}