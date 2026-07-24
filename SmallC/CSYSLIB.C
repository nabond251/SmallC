/*
** CSYSLIB - System-Level Library Functions
*/

#include "stdio.h"
#include "clib.h"

/*
****************** System Variables ********************
*/

int
  _cnt=1,             /* arg count for main */
  _vec[20],           /* arg vectors for main */
  _status[MAXFILES] = {OPNBIT, OPNBIT, OPNBIT, OPNBIT, OPNBIT},
  _cons  [MAXFILES],  /* fast way to tell if file is the console */
  _nextc [MAXFILES] = {EOF, EOF, EOF, EOF, EOF},
  _bufuse[MAXFILES],  /* current buffer usage: NULL, EMPTY, IN, OUT */
  _bufsiz[MAXFILES],  /* size of buffer */
  _bufptr[MAXFILES],  /* aux buffer address */
  _bufnxt[MAXFILES],  /* address of next byte in buffer */
  _bufend[MAXFILES],  /* address of end-of-data in buffer */
  _bufeof[MAXFILES];  /* true if current buffer ends file */

char
 *_memptr,           /* pointer to free memory. */
  _arg1[]="*";       /* first arg for main */

/*
*************** System-Level Functions *****************
*/

/*
**  Process command line, allocate default buffer to each fd,
**  execute main(), and exit to DOS.  Must be executed with es=psp.
**  Small default buffers are allocated because a high price is paid for
**  byte-by-byte calls to DOS.  Tests gave these results for a simple
**  copy program:
**
**          chunk size       copy time in seconds
**              1                    36
**              5                    12
**             25                     6
**             50                     6
*/
_main() {
  int fd;
  _parse();
  for(fd = 0; fd < MAXFILES; ++fd) auxbuf(fd, 32);
  if(!isatty(stdin)) _bufuse[stdin]   = EMPTY;
  if(!isatty(stdout)) _bufuse[stdout] = EMPTY;
  main(_cnt, _vec);
  exit(0);
  }

/*
** Parse command line and setup argc and argv.
** Must be executed with es == psp
*/
_parse() {
  char *ptr;
#asm
  mov     cl,es:[80h]  ; get parameter string length
  mov     ch,0
  push    cx           ; save it
  inc     cx
  push    cx           ; 1st __alloc() arg
  mov     ax,1
  push    ax           ; 2nd __alloc() arg
  call    __alloc      ; allocate zeroed memory for args
  add     sp,4
  mov     [bp-2],ax    ; ptr = addr of allocated memory
  pop     cx
  push    es           ; exchange
  push    ds           ; es         (source)
  pop     es           ;    and
  pop     ds           ;        ds  (destination)
  mov     si,81h       ; source offset
  mov     di,[bp-2]    ; destination offset
  rep     movsb        ; move string
  mov     al,0
  stosb                ; terminate with null byte
  push    es
  pop     ds           ; restore ds
#endasm
  _vec[0]=_arg1;       /* first arg = "*" */
  while (*ptr) {
    if(isspace(*ptr)) {++ptr; continue;}
    if(_cnt < 20) _vec[_cnt++] = ptr;
    while(*ptr) {
      if(isspace(*ptr)) {*ptr = NULL; ++ptr; break;}
      ++ptr;
      }
    }
  }

/*
** Binary-Stream output of one byte to fd.
*/
_write(ch, fd) int ch, fd; {
  if(_bufuse[fd]) return(_writebuf(ch, fd));
  if(_bdos2(WRITE<<8, fd, 1, &ch) != 1) {
    _seterr(fd);
    return (EOF);
    }
  return (ch);
  }

/*
** Empty buffer if necessary, and store ch in buffer.
*/
_writebuf(ch, fd) int ch, fd; {
  char *ptr;
  if(_bufuse[fd] == IN && _backup(fd)) return (EOF);
  while(YES) {
    ptr = _bufnxt[fd];
    if(ptr < (_bufptr[fd] + _bufsiz[fd])) {
      *ptr = ch;
      ++_bufnxt[fd];
      _bufuse[fd] = OUT;
      return (ch);
      }
    if(_flush(fd)) return (EOF);
    }
  }

/*
** Flush buffer to DOS if dirty buffer.
** Reset buffer pointers in any case.
*/
_flush(fd) int fd; {
  int i, j, k, chunk;
  if(_bufuse[fd] == OUT) {
    i = _bufnxt[fd] - _bufptr[fd];
    k = 0;
    while(i > 0) {     /* avoid DMA problem on physical 64K boundary */
      if(i < 512) chunk = i;
      else        chunk = 512;
      k += (j = _bdos2(WRITE<<8, fd, chunk, _bufptr[fd] + k));
      if (j < chunk) {_seterr(fd); return (EOF);}
      i -= j;
      }
    }
  _empty(fd, YES);
  return (NULL);
  }

/*
** Allocate n bytes of (possibly zeroed) memory.
** Entry: n = Size of the items in bytes.
**    clear = "true" if clearing is desired.
** Returns the address of the allocated block of memory
** or NULL if the requested amount of space is not available.
*/
_alloc(n, clear) unsigned n, clear; {
  char *oldptr;
  if(n < avail(YES)) {
    if(clear) pad(_memptr, NULL, n);
    oldptr = _memptr;
    _memptr += n;
    return (oldptr);
    }
  return (NULL);
  }
