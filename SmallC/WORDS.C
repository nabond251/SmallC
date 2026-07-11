/* words -- put every word on a line by itself */
#include <stdio.h>
#define INSIDE  1
#define OUTSIDE 0
char ch;
int where = OUTSIDE;
main() {
  while((ch = getchar()) != EOF) {
    if((ch == ' ') || (ch == '\n') || (ch == '\t'))
         white();
    else black();
    }
  }
white() {
  if (where == INSIDE) putchar('\n');
  where = OUTSIDE;
  }
black() {
  putchar(ch);
  where = INSIDE;
  }