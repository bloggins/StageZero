#!/usr/bin/env python3

import argparse
import base64
import sys


def main():
    parser = argparse.ArgumentParser(
        description="Convert a binary file to Base64."
    )
    parser.add_argument("input", help="Input .bin file")
    parser.add_argument("-o", "--output", help="Output Base64 file")
    args = parser.parse_args()

    with open(args.input, "rb") as f:
        data = f.read()

    encoded = base64.b64encode(data).decode("ascii")

    if args.output:
        with open(args.output, "w", encoding="ascii") as f:
            f.write(encoded)
        print(f"Base64 written to: {args.output}")
    else:
        print(encoded)


if __name__ == "__main__":
    main()