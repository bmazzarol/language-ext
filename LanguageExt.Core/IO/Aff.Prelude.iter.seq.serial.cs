using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using LanguageExt.Interfaces;
using LanguageExt.Thunks;

namespace LanguageExt
{
    public static partial class Prelude
    {
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<A>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = await f(x).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<A>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = await f(x).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<A>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = f(x).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<A>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = f(x).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<A>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = await f(x).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Unit> iter<A>(Aff<Seq<A>> ma, Func<A, Aff<Unit>> f) =>
            AffMaybe(async () =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = await f(x).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<A>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = f(x).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Unit> iter<A>(Aff<Seq<A>> ma, Func<A, Eff<Unit>> f) =>
            AffMaybe(async () =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var x in xs.Value)
                {
                    var r = f(x).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
        

        
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<Env, A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<Env, A>>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<Env, A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<Env, A>>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Aff<Env, A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Aff<Env, A>>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Aff<Env, A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Aff<Env, A>>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO(env).ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });        
         

        
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<A>>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Aff<A>>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Aff<A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Unit> iter<A>(Aff<Seq<Aff<A>>> ma, Func<A, Aff<Unit>> f) =>
            AffMaybe(async () =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Aff<A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Unit> iter<A>(Aff<Seq<Aff<A>>> ma, Func<A, Eff<Unit>> f) =>
            AffMaybe(async () =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = await iox.RunIO().ConfigureAwait(false);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });   
    
    
    
    
            
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<Env, A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<Env, A>>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<Env, A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<Env, A>>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Eff<Env, A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Eff<Env, A>>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Eff<Env, A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Eff<Env, A>>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO(env);
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });        
         
    
        
 
    
            
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<A>>> ma, Func<A, Aff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Env, Seq<Eff<A>>> ma, Func<A, Eff<Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO(env).ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
        
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Eff<A>>> ma, Func<A, Aff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO(env).ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });

        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Unit> iter<A>(Aff<Seq<Eff<A>>> ma, Func<A, Aff<Unit>> f) =>
            AffMaybe(async () =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = await f(x.Value).RunIO().ConfigureAwait(false);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Env, Unit> iter<Env, A>(Aff<Seq<Eff<A>>> ma, Func<A, Eff<Env, Unit>> f) where Env : struct, HasCancel<Env> =>
            AffMaybe<Env, Unit>(async env =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO(env);
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });
 
        /// <summary>
        /// Sequentially iterate items in a collection
        /// </summary>
        /// <param name="ma">Collection to iterate</param>
        /// <param name="f">Function to apply to each item in the collection</param>
        /// <returns>Unit</returns>
        public static Aff<Unit> iter<A>(Aff<Seq<Eff<A>>> ma, Func<A, Eff<Unit>> f) =>
            AffMaybe(async () =>
            {
                var xs = await ma.RunIO().ConfigureAwait(false);
                if (xs.IsFail) return xs.Cast<Unit>();

                foreach (var iox in xs.Value)
                {
                    var x = iox.RunIO();
                    if (x.IsFail) return x.Cast<Unit>();
                    var r = f(x.Value).RunIO();
                    if (r.IsFail) return r.Cast<Unit>();
                }

                return Fin<Unit>.Succ(default);
            });        
        
    }
}
