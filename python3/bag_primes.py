#!/usr/bin/env python3

print(f"{__file__} here")


class Bag(object):
    _primes = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101]

    __slots__ = 'num'

    def __init__(self, str):
        self.num = 1

        for c in str.lower():
            if (c >= 'a') and (c <= 'z'):
                self.num *= self._primes[ord(c) - ord('a')]

    def empty(self):
        return self.num == 1

    def __repr__(self):
        return repr(self.num)

    def __eq__(self, other):
        return self.num == other.num

    def __hash__(self):
        return self.num.__hash__()

    def __sub__(self, other):
        quotient, remainder = divmod(self.num, other.num)
        if (0 == remainder):
            rv = Bag("")
            rv.num = quotient
            return rv
        else:
            return None
