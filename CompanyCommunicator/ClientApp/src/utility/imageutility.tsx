import ColorHash from "color-hash";

export class ImageUtil {

    public static makeInitialImage = (name: string) => {
        const canvas = document.createElement('canvas');
        canvas.style.display = 'none';
        canvas.width = 32;
        canvas.height = 32;
        document.body.appendChild(canvas);
        const context = canvas.getContext('2d');
        if (context) {
            let colorHash = new ColorHash();
            const colorNum = colorHash.hex(name);
            context.fillStyle = colorNum;
            context.fillRect(0, 0, canvas.width, canvas.height);
            context.font = "14px Arial";
            context.fillStyle = "#fff";
            const split = name.split(' ');
            const len = split.length;
            const first = split[0][0];
            if (len > 1) {
                const last = split[len - 1][0];
                const initials = first + last;
                context.fillText(initials.toUpperCase(), 6, 21);
            } else {
                context.fillText(first.toUpperCase(), 10, 21);
            }
            const data = canvas.toDataURL();
            document.body.removeChild(canvas);
            return data;
        } else {
            return "";
        }
    }
}