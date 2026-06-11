import { useEffect, useRef } from "react";
import cornerstone from "cornerstone-core";
import cornerstoneWADOImageLoader from "cornerstone-wado-image-loader";
import dicomParser from "dicom-parser";

cornerstoneWADOImageLoader.external.cornerstone = cornerstone;
cornerstoneWADOImageLoader.external.dicomParser = dicomParser;
cornerstoneWADOImageLoader.configure({
    useWebWorkers: false
});

function DicomViewer({ file }) {
    const elementRef = useRef(null);

    useEffect(() => {
        if (!file) return;

        const element = elementRef.current;

        cornerstone.enable(element);

        const imageId =
            cornerstoneWADOImageLoader.wadouri.fileManager.add(file);

        console.log("ImageId:", imageId);

        cornerstone.loadAndCacheImage(imageId)
            .then((image) => {

                console.log("Loaded image", image);

                cornerstone.displayImage(
                    element,
                    image
                );

            })
            .catch((err) => {

                console.error(
                    "DICOM LOAD ERROR:",
                    err
                );

            });

        return () => {
            cornerstone.disable(element);
        };
    }, [file]);

    return (
        <div
            ref={elementRef}
            style={{
                width: "100%",
                height: "500px",
                border: "1px solid #ccc",
                borderRadius: "12px"
            }}
        />
    );
}

export default DicomViewer;